using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Controls all cauldron visual effects:
///   - Swirling liquid shader (surface mesh material)
///   - Bubbling particle system with intensity scaling
///   - Pour stream + splash via VFX Graph
///   - Ingredient color mixing
///
/// Setup:
///   1. Assign liquidMaterial (the material on your flat surface mesh inside the cauldron).
///      Shader must expose: _SwirlSpeed (float), _MixColor (Color),
///      _RippleStrength (float 0-1), _BoilDistortion (float 0-1).
///   2. Assign bubbleSystem (a ParticleSystem sitting inside the cauldron).
///      Add a sub-emitter (Death event) for bubble pop particles.
///   3. Assign pourVFX (a VisualEffect component, your VFX Graph asset).
///      Graph must expose: PourPosition (Vector3), events OnPourStart / OnPourStop.
/// </summary>
public class CauldronController : MonoBehaviour
{
    // ─── Inspector References ───────────────────────────────────────────────

    [Header("Liquid Surface")]
    [Tooltip("Material on the flat mesh that represents the liquid surface.")]
    public Material liquidMaterial;

    [Header("Bubble System")]
    [Tooltip("ParticleSystem for rising bubbles. Add a sub-emitter (Death) for pops.")]
    public ParticleSystem bubbleSystem;

    [Header("Pour VFX")]
    [Tooltip("VisualEffect component for the pour stream and splash burst.")]
    public VisualEffect pourVFX;

    // For liquid tilt (anti-clip) effect - see UpdateLiquidTilt() method

    [Header("Liquid Tilt (Anti-Clip)")]
    [Tooltip("The liquid surface Transform to tilt (should be a child of the cauldron).")]
    public Transform liquidSurface;
    public Transform spoutTransform;
    public Transform splashPlane;

    [Tooltip("Maximum tilt angle in degrees before clipping the cauldron walls.")]
    public float maxTiltAngle = 4f;

    [Tooltip("How smoothly the liquid follows movement.")]
    public float tiltDamping = 4f;

    [Tooltip("How strongly acceleration causes tilt.")]
    public float tiltSensitivity = 0.8f;

    // ─── Shader Property IDs (cached for performance) ────────────────────────

    static readonly int SwirlSpeedID      = Shader.PropertyToID("_SwirlSpeed");
    static readonly int MixColorID        = Shader.PropertyToID("_MixColor");
    static readonly int RippleStrengthID  = Shader.PropertyToID("_RippleStrength");
    static readonly int BoilDistortionID  = Shader.PropertyToID("_BoilDistortion");

    // ─── Tuning ──────────────────────────────────────────────────────────────

    [Header("Swirl Settings")]
    [Tooltip("Base rotation speed of the liquid surface swirl at heat = 0.")]
    public float baseSwirlSpeed = 0.3f;
    [Tooltip("Max rotation speed added at full heat.")]
    public float maxSwirlBoost  = 1.2f;

    [Header("Bubble Settings")]
    [Tooltip("Bubble emission rate at full heat (heat = 1).")]
    public float maxBubbleRate  = 80f;
    [Tooltip("Bubble speed multiplier at full heat.")]
    public float maxBubbleSpeed = 1.5f;

    [Header("Ripple Settings")]
    [Tooltip("How quickly the splash ripple fades out after a pour.")]
    public float rippleFadeSpeed = 2f;

    [Header("Ingredient Color")]
    [Tooltip("How fast the liquid color lerps toward a new ingredient color.")]
    public float colorLerpSpeed = 0.5f;

    // ─── State ───────────────────────────────────────────────────────────────

    float   _currentHeat;       // 0 = cold, 1 = boiling
    Color _baseColor;        // initial liquid color (from material)
    Color   _currentColor;      // current liquid tint
    Color   _targetColor;       // target after ingredient added
    bool    _pouring;
    Coroutine _rippleFadeRoutine;

    Vector3 _lastPosition;
    Vector3 _targetTilt; // euler X and Z tilt in degrees

    // ─── Unity Lifecycle ─────────────────────────────────────────────────────

    void Awake()
    {
        if (liquidMaterial != null)
            _currentColor = _targetColor = liquidMaterial.GetColor(MixColorID);
            _baseColor = _currentColor;
        _lastPosition = transform.position;
    }

    void Update()
    {
        UpdateLiquidTilt();
        // Smoothly lerp liquid color toward target (ingredient mixing)
        if (_currentColor != _targetColor)
        {
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * colorLerpSpeed);
            liquidMaterial?.SetColor(MixColorID, _currentColor);

            if (Vector4.Distance(_currentColor, _targetColor) < 0.001f)
                _currentColor = _targetColor;
        }
    }

    void UpdateLiquidTilt()
    {
        if (liquidSurface == null) return;

        // Measure how much the cauldron moved this frame
        Vector3 velocity    = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition       = transform.position;

        // Tilt opposes the direction of movement (liquid lags behind)
        float tiltX = Mathf.Clamp(-velocity.y * tiltSensitivity, -maxTiltAngle, maxTiltAngle);
        float tiltZ = Mathf.Clamp( velocity.x * tiltSensitivity, -maxTiltAngle, maxTiltAngle);

        _targetTilt = new Vector3(tiltX, 0f, tiltZ);

        liquidSurface.localRotation = Quaternion.Lerp(
            liquidSurface.localRotation,
            Quaternion.Euler(_targetTilt),
            Time.deltaTime * tiltDamping
        );
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void ResetCauldron()
    {
        SetHeat(0f);
        _targetColor = _baseColor;
        if (liquidMaterial != null)
        {
            liquidMaterial.SetFloat(SwirlSpeedID, baseSwirlSpeed);
            liquidMaterial.SetFloat(BoilDistortionID, 0f);
            liquidMaterial.SetFloat(RippleStrengthID, 0f);
        }
    }

    /// <summary>
    /// Drive the heat level of the cauldron (0 = cold, 1 = boiling).
    /// Scales bubble rate, bubble speed, swirl speed, and boil distortion.
    /// Call from an animation, trigger zone, or game state manager.
    /// </summary>
    public void SetHeat(float heat01)
    {
        _currentHeat = Mathf.Clamp01(heat01);

        // ── Shader ──
        if (liquidMaterial != null)
        {
            liquidMaterial.SetFloat(SwirlSpeedID, baseSwirlSpeed + _currentHeat * maxSwirlBoost);
            liquidMaterial.SetFloat(BoilDistortionID, _currentHeat * 0.3f);
        }

        // ── Bubble emission ──
        if (bubbleSystem != null)
        {
            var emission = bubbleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(0f, maxBubbleRate, _currentHeat);

            var velocityModule = bubbleSystem.velocityOverLifetime;
            velocityModule.speedModifier = Mathf.Lerp(0.3f, maxBubbleSpeed, _currentHeat);

            // Stop / restart the system when transitioning cold ↔ warm
            if (_currentHeat > 0.01f && !bubbleSystem.isPlaying)
                bubbleSystem.Play();
            else if (_currentHeat <= 0.01f && bubbleSystem.isPlaying)
                bubbleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void StartBoiling() => SetHeat(1f);
    public void StopBoiling()  => SetHeat(0f);

    // Starting swirling the cauldron liquid (OnClickStir in Reaction.cs)
    public void StartMixing() => StartCoroutine(SwirlCoroutine());

    /// <summary>
    /// Begin pouring liquid into the cauldron from a world-space position.
    /// Activates the VFX Graph pour stream.
    /// </summary>
    public void StartPour()
    {
        if (pourVFX == null) return;

        if (spoutTransform == null) return;
        pourVFX.SetVector3("PourPosition", spoutTransform.position);

        if (splashPlane == null) return;
        pourVFX.SetVector3("SplashPlanePosition", splashPlane.position);
        _pouring = true;
        pourVFX.SendEvent("OnPourStart");
        StartCoroutine(PourStreamCoroutine());
    }

    /// <summary>
    /// Stop the pour stream and trigger a splash ripple on the liquid surface.
    /// </summary>
    public void StopPour()
    {
        if (pourVFX == null) return;

        if (!_pouring) return;

        Debug.Log("Stop Pouring");
        _pouring = false;

        pourVFX.SendEvent("OnPourStop");
        pourVFX.Reinit();
        // Trigger ripple on the liquid shader
        if (_rippleFadeRoutine != null)
            StopCoroutine(_rippleFadeRoutine);
        _rippleFadeRoutine = StartCoroutine(FadeRipple());
    }

    /// <summary>
    /// Add an ingredient, blending its color into the liquid.
    /// Temporarily boosts swirl speed for a "mixing" moment.
    /// </summary>
    public void AddIngredient(string ingredientColor)
    {
        _targetColor = Color.black;
        ColorUtility.TryParseHtmlString(ingredientColor, out _targetColor);
        StartCoroutine(MixingSwirlBoost());
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────

    /// Fades the ripple shader parameter back to zero after a splash.
    IEnumerator FadeRipple()
    {
        liquidMaterial?.SetFloat(RippleStrengthID, 1f);

        float strength = 1f;
        while (strength > 0f)
        {
            strength -= Time.deltaTime * rippleFadeSpeed;
            liquidMaterial?.SetFloat(RippleStrengthID, Mathf.Max(0f, strength));
            yield return null;
        }
    }

    /// Briefly boosts swirl speed when an ingredient is added, then settles back.
    IEnumerator MixingSwirlBoost()
    {
        float boostDuration  = 1.5f;
        float boostMagnitude = 1.8f;
        float elapsed        = 0f;
        float baseSpeed      = baseSwirlSpeed + _currentHeat * maxSwirlBoost;

        while (elapsed < boostDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / boostDuration;

            // Rise quickly, fade out with ease-out curve
            float boost = boostMagnitude * Mathf.Pow(1f - t, 2f);
            liquidMaterial?.SetFloat(SwirlSpeedID, baseSpeed + boost);
            yield return null;
        }

        // Restore normal speed
        liquidMaterial?.SetFloat(SwirlSpeedID, baseSpeed);
    }

    IEnumerator SwirlCoroutine()
    {
        float elapsed = 0f;
        float swirlDuration = 4f;
        float swirlMagnitude = 0.5f;
        while (elapsed < swirlDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swirlDuration;

            SetHeat(Mathf.Lerp(_currentHeat, 1f, t)); // Gradually increase heat to boiling
            liquidMaterial?.SetFloat(SwirlSpeedID, baseSwirlSpeed + _currentHeat 
                * maxSwirlBoost + swirlMagnitude * Mathf.Sin(t * Mathf.PI * 4));
            yield return null;
        }

        // Restore normal speed
        liquidMaterial?.SetFloat(SwirlSpeedID, baseSwirlSpeed + _currentHeat * maxSwirlBoost);
        SetHeat(0f); // Cool down after stirring
    }

    IEnumerator PourStreamCoroutine()
    {
        if (pourVFX == null) yield break;
        Debug.Log($"Pour Routine Start");

        float elapsed = 0f;
        float pourDuration = 3f; // Max duration for the pour stream (can be interrupted by StopPour)

        pourVFX.pause = false;

        // Keep the pour stream active until StopPour is called
        while (elapsed < pourDuration)
        {
            elapsed += Time.deltaTime;
            pourVFX.SendEvent("UpdatePour"); // Custom event to update pour position if needed
            yield return null;
        }

        pourVFX.pause = true;
        StopPour();
    }

    // ─── Editor Helpers ───────────────────────────────────────────────────────

#if UNITY_EDITOR
    [Header("Editor Testing")]
    [Range(0f, 1f)] public float debugHeat;
    public Color debugIngredientColor = Color.green;

    void OnValidate()
    {
        // Live-preview heat level changes in the editor
        if (Application.isPlaying)
            SetHeat(debugHeat);
    }
#endif
}