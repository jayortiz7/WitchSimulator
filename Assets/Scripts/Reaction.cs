// each ingredient is a button
// exactly 3 ingredients must be added, prompt user to add more if < 3. must be different
// ingredients, so if user tries to click an ingredient already in set,
// have a pop up that says "choose a different ingredient"
// and once 3 are added, there is a button that pops up that says stir
// once its stirred, it will turn a unqiue color based on the type of potion.
// then once stirred a button will pop up to have the witch drink it.
// based on the type of potion (determined by which 3 ingredients), the witch will have a
// unique reaction
// use Materials for the 3D liquid mesh in the cauldron — drag each Material into the
// corresponding field in the Inspector
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Reactions : MonoBehaviour
{
    private HashSet<string> ingredients = new HashSet<string>();
    private const int MAX_INGREDIENTS = 3;
    private Coroutine feedbackCoroutine;
    private bool reactionComplete = false;
    private SkinnedMeshRenderer[] witchRenderers;
    private Color[] originalColors;
    private Material[] originalMaterials;
    private Quaternion originalRotation;
    private Vector3 originalWitchPosition;
    private Vector3 originalScale;

    // Materials for the 3D cauldron liquid — assign each in the Inspector
    
    public Material defaultPotionMaterial;   // the starting liquid material
    public Material turnToStone;

    // The Renderer on the 3D liquid mesh inside the cauldron
    public Renderer potionLiquidRenderer;

    public Button drinkButton;
    public Button stirButton;
    //public Button boilButton;
    public Button resetButton;
    public TMP_Text feedbackText;
    private Animator animator;
    public TMP_Text ingredientsText;

    // Inspector references to cauldron
    public GameObject cauldron;
    public ParticleSystem sleepParticles;
    public ParticleSystem loveParticles;
    public ParticleSystem smokeScreen;
    public ParticleSystem explosionParticles;
    public GameObject goblinCharacter;
    public GameObject witchCharacter;
    public GameObject animalCharacter;

    // Potion bottle held in the witch's hand while pouring
    [Header("Potion Bottle")]
    public GameObject potionBottlePrefab;
    public Vector3 bottleHandOffset;
    public Vector3 bottleHandRotation;
    public Vector3 bottleScale = Vector3.one;
    private GameObject activeBottle;





    void Start()
    {
        stirButton.gameObject.SetActive(false);
        //boilButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        if (defaultPotionMaterial != null)
            potionLiquidRenderer.material = defaultPotionMaterial;
        feedbackText.text = "Add 3 ingredients!";
        animator = GetComponent<Animator>();
        originalRotation = witchCharacter.transform.rotation;
        originalWitchPosition = witchCharacter.transform.position;
        originalScale = witchCharacter.transform.localScale;
        cauldron = GameObject.Find("Cauldron");
        // for color changing witch, get all her components
        witchRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        originalColors = new Color[witchRenderers.Length];
        originalMaterials = new Material[witchRenderers.Length];
        for (int i = 0; i < witchRenderers.Length; i++)
        {
            witchRenderers[i].material = new Material(witchRenderers[i].sharedMaterial);
            originalColors[i] = witchRenderers[i].material.color;
            originalMaterials[i] = witchRenderers[i].material;
        }
    }

    // for demoing and testing only
    void Update(){
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(TurnToStone());
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "You turned the witch to stone! Look at her, she's a real rockstar!";
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(TurnPurple());
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "You turned her purple! Barney who?";
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            sleepParticles.Play();
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Sleep potion! Nighty night...";
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            loveParticles.Play();
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Love potion! I think she's into you...";
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetTrigger("Levitate");
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Levitation potion! Up up and awayyy";
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(ShrinkDown());
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "You shrunk the witch!";
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Flex");
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Strength potion! She might be invincible!";
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            animator.SetTrigger("Spin");
            Invoke("SwapToGoblin", 4.0f);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Goblin potion! Gone full goblin mode!";
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            smokeScreen.Play();
            Invoke("SwapToAnimal", 2.0f);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Animal potion! MOOOOO";
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            explosionParticles.Play();
            StartCoroutine(Disappear());
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Explosion potion! YOU BLEW HER UP";
        }
    }

    private string GetIngredientColor(string ingredient)
    {
        switch (ingredient)
        {
            case "Dragon's Blood": return "#650404";
            case "Unicorn Tears": return "#ADD8E6";
            case "Black Magic Bean Juice": return "#1A1A1A";
            case "Goblin Sweat": return "#9ACD32";
            case "Bat Drool": return "#785b3e";
            default: return "#FFFFFF";
        }
    }

    // Spawns a potion bottle in the witch's hand, tinted to the ingredient's color,
    // and removes it once the pour finishes.
    private void SpawnPotionBottle(string ingredient)
    {
        if (potionBottlePrefab == null || animator == null) return;

        Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand == null) return;

        if (activeBottle != null) Destroy(activeBottle);

        activeBottle = Instantiate(potionBottlePrefab, hand);
        activeBottle.transform.localPosition = bottleHandOffset;
        activeBottle.transform.localEulerAngles = bottleHandRotation;
        activeBottle.transform.localScale = bottleScale;

        // Prevent physics from pulling the bottle out of the witch's hand
        foreach (var rb in activeBottle.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
        foreach (var col in activeBottle.GetComponentsInChildren<Collider>())
            Destroy(col);

        Color bottleColor;
        if (ColorUtility.TryParseHtmlString(GetIngredientColor(ingredient), out bottleColor))
        {
            var propertyBlock = new MaterialPropertyBlock();
            foreach (var renderer in activeBottle.GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", bottleColor);
                propertyBlock.SetColor("_Color", bottleColor);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        StartCoroutine(RemoveBottleAfterPour(activeBottle, 3f));
    }

    private IEnumerator RemoveBottleAfterPour(GameObject bottle, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bottle == activeBottle) activeBottle = null;
        if (bottle != null) Destroy(bottle);
    }

    private IEnumerator StartPourAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        cauldron.GetComponent<CauldronController>().StartPour();
    }

    public void OnIngredientClicked(string ingredient) {

        // if animation happened, immediately return
        if (reactionComplete){

            return;
        }

        if (ingredients.Count >= MAX_INGREDIENTS) {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Cauldron full, click stir";
            return;
        }
        if (ingredients.Contains(ingredient)) {
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShowTempFeedback("Add a different ingredient!", 3f));
            return;
        }

        //boilButton.gameObject.SetActive(true);
        StartCoroutine(StartPourAfterDelay(0.5f));
        SpawnPotionBottle(ingredient);

        animator.SetTrigger("AddIngredient");

        // if not already in ingredients, add to ingredients
        ingredients.Add(ingredient);
        UpdateIngredientsDisplay();
        if (ingredients.Count == MAX_INGREDIENTS)
        {
            //boilButton.gameObject.SetActive(false);
            StartCoroutine(ShowStirAfterAddIngredientAnimation());
            return;
        }
    }

    private IEnumerator ShowStirAfterAddIngredientAnimation()
    {
        // wait a frame for the animator to transition into the AddIngredient state
        yield return null;
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("PickState"));

        // PickState exits (back to idle) at 90% through its clip, so wait that long
        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        float clipLength = clipInfo.Length > 0 ? clipInfo[0].clip.length : 1f;
        yield return new WaitForSeconds(clipLength * 0.9f);

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "Click stir!";
        stirButton.gameObject.SetActive(true);
    }


    public void OnStirClicked()
    {
        Debug.Log("Clicked stir");
        animator.SetTrigger("Stir");
        cauldron.GetComponent<CauldronController>().StartMixing();

        stirButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        StartCoroutine(ShowDrinkButtonAfterDelay(3f));
    }

    private IEnumerator ShowDrinkButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        drinkButton.gameObject.SetActive(true);
    }

    public void OnDrinkClicked()
    {
        animator.SetTrigger("Drink");
        drinkButton.gameObject.SetActive(false);
        StartCoroutine(WaitThen(() => {
        string reactionText;
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // love potion reaction
            loveParticles.Play();
            reactionText = "Love potion! I think she's into you...";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool")) {
            StartCoroutine(ShrinkDown());
            reactionText = "You shrunk the witch!";
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // sleeping reaction
            sleepParticles.Play();
            reactionText = "Sleep potion! Nighty night...";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice")) {
            // explosion reaction
            explosionParticles.Play();
            StartCoroutine(Disappear());
            reactionText = "Explosion potion! YOU BLEW HER UP";
        }
        else if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into animal reaction
            smokeScreen.Play();
            Invoke("SwapToAnimal", 2.0f);
            reactionText = "Animal potion! MOOOOO";
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into stone reaction
            StartCoroutine(TurnToStone());
            reactionText = "You turned the witch to stone! Look at her, she's a real rockstar!";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // levitation reaction
            animator.SetTrigger("Levitate");
            reactionText = "Levitation potion! Up up and awayyy";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // strength reaction
            animator.SetTrigger("Flex");
            reactionText = "Strength potion! She might be invincible!";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears")) {
            // turn purple reaction
            // coroutine moves a little each frame for couple seconds
            StartCoroutine(TurnPurple());
            reactionText = "You turned her purple! Barney who?";
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into goblin reaction
            animator.SetTrigger("Spin");
            Invoke("SwapToGoblin", 4.0f);
            reactionText = "Goblin potion! Gone full goblin mode!";
        }
        else {
            reactionText = "";
        }

        reactionComplete = true;
        StartCoroutine(ShowReactionTextAfterDelay(reactionText, 2f));
        StartCoroutine(ShowReactionCompleteText(6f));
        }));
    }

    private IEnumerator ShowReactionTextAfterDelay(string text, float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = text;
    }

    private IEnumerator WaitThen(System.Action action)
    {
        yield return new WaitForSeconds(5f);
        action();
    }

    public void OnResetClicked()
    {
        ingredients.Clear();
        reactionComplete = false;
        UpdateIngredientsDisplay();
        stirButton.gameObject.SetActive(false);
        //boilButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "Add 3 ingredients!";
        animator.speed = 1f;
        for (int i = 0; i < witchRenderers.Length; i++)
            witchRenderers[i].material = originalMaterials[i];
        StartCoroutine(ResetColor());
        loveParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        sleepParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (defaultPotionMaterial != null) {
            potionLiquidRenderer.material = defaultPotionMaterial;
        }
        foreach (var r in witchRenderers)
        {
            r.enabled = true;
        }
        goblinCharacter.SetActive(false);
        animalCharacter.SetActive(false);
        animator = GetComponent<Animator>();
        witchCharacter.SetActive(true);
        witchCharacter.transform.SetPositionAndRotation(originalWitchPosition, originalRotation);
        witchCharacter.transform.localScale = originalScale;
    }

    void SwapToGoblin() {
        StartCoroutine(SmoothSwap());
    }

    private IEnumerator SmoothSwap()
    {
        float currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
        goblinCharacter.SetActive(true);
        goblinCharacter.transform.rotation = witchCharacter.transform.rotation;
        animator = goblinCharacter.GetComponent<Animator>();
        animator.Play("Spin", 0, currentTime);

        yield return null;
        witchCharacter.SetActive(false);
    }

    private IEnumerator TurnPurple()
    {
        Color purple = new Color(0.5f, 0f, 1f);
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < witchRenderers.Length; i++)
                witchRenderers[i].material.color = Color.Lerp(originalColors[i], purple, t);
            yield return null;
        }
    }

    private IEnumerator ShrinkDown()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            witchCharacter.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.17f, t);
            yield return null;
        }
    }

    private IEnumerator TurnToStone()
    {
        Color stone = new Color(0.4f, 0.4f, 0.4f);
        float duration = 1.9f;
        float elapsed = 0f;

        Debug.Log("witchRenderers count: " + witchRenderers.Length);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < witchRenderers.Length; i++)
                witchRenderers[i].material.color = Color.Lerp(originalColors[i], stone, t * t);
            animator.speed = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        foreach (var r in witchRenderers)
            r.material = turnToStone;

        yield return new WaitForSeconds(0.2f);

        // find feet position from mesh bounds
        float minY = float.MaxValue;
        foreach (var r in witchRenderers)
            if (r.bounds.min.y < minY) minY = r.bounds.min.y;
        Vector3 feetPos = new Vector3(witchCharacter.transform.position.x, minY + 1.7f, witchCharacter.transform.position.z);

        // wobble with increasing amplitude, track exit velocity
        float wobbleDuration = 2.8f;
        float wobbleAmplitude = 4f;
        float wobblePhase = Random.Range(0f, Mathf.PI * 2f);
        elapsed = 0f;
        float lastAngle = 0f;
        float exitVelocity = 0f;
        while (elapsed < wobbleDuration)
        {
            float prevAngle = lastAngle;
            elapsed += Time.deltaTime;
            float buildup = elapsed / wobbleDuration;
            float angle = Mathf.Sin(elapsed * 1.5f * Mathf.PI * 2f + wobblePhase) * wobbleAmplitude * buildup;
            float delta = angle - prevAngle;
            witchCharacter.transform.RotateAround(feetPos, Vector3.forward, delta);
            exitVelocity = delta / Time.deltaTime;
            lastAngle = angle;
            yield return null;
        }
        float fallDir = lastAngle >= 0f ? 1f : -1f;

        // topple with initial velocity matching wobble exit
        float toppleDuration = 0.4f;
        float toppleAngle = 85f;
        float targetDelta = toppleAngle * fallDir;
        float accel = 2f * (targetDelta - exitVelocity * toppleDuration) / (toppleDuration * toppleDuration);
        float rotated = lastAngle;
        elapsed = 0f;
        while (elapsed < toppleDuration)
        {
            elapsed += Time.deltaTime;
            float target = lastAngle + exitVelocity * elapsed + 0.5f * accel * elapsed * elapsed;
            float delta = target - rotated;
            witchCharacter.transform.RotateAround(feetPos, Vector3.forward, delta);
            rotated = target;
            yield return null;
        }
    }

    private IEnumerator ResetColor()
    {
        float duration = 1f;
        float elapsed = 0f;
        Color[] currentColors = new Color[witchRenderers.Length];
        for (int i = 0; i < witchRenderers.Length; i++)
            currentColors[i] = witchRenderers[i].material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < witchRenderers.Length; i++)
                witchRenderers[i].material.color = Color.Lerp(currentColors[i], originalColors[i], t);
            yield return null;
        }
    }

    void SwapToAnimal() {
        StartCoroutine(SmoothAnimalSwap());
    }

    private IEnumerator SmoothAnimalSwap()
    {
        animalCharacter.SetActive(true);
        // animalCharacter.transform.position = witchCharacter.transform.position;
        // animalCharacter.transform.rotation = witchCharacter.transform.rotation;
        yield return null;
        witchCharacter.SetActive(false);
        smokeScreen.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator ShowTempFeedback(string message, float duration)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        yield return new WaitForSeconds(duration);
        feedbackText.text = ingredients.Count >= MAX_INGREDIENTS ? "Click stir!" : "Add 3 ingredients!";
    }

    // wait a few seconds b4 prompting user to play again
    private IEnumerator ShowReactionCompleteText(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "Click reset to play again!";
    }

    private void UpdateIngredientsDisplay()
{
    ingredientsText.text = "In the cauldron...\n";
    foreach (string ingredient in ingredients)
    {
        ingredientsText.text += ingredient + "\n";
    }
}

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(0.9f);
        foreach (var r in witchRenderers) {
            r.enabled = false;
        }
        explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

}
