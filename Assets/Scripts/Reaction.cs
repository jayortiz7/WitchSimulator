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
    private SkinnedMeshRenderer[] witchRenderers;
    private Color[] originalColors;

    // Materials for the 3D cauldron liquid — assign each in the Inspector
    public Material lovePotionMaterial;
    public Material shrinkingMaterial;
    public Material sleepingMaterial;
    public Material explosionMaterial;
    public Material animalMaterial;
    public Material stoneMaterial;
    public Material levitationMaterial;
    public Material strengthMaterial;
    public Material purpleMaterial;
    public Material goblinMaterial;
    public Material defaultPotionMaterial;   // the starting liquid material

    // The Renderer on the 3D liquid mesh inside the cauldron
    public Renderer potionLiquidRenderer;

    public Button drinkButton;
    public Button stirButton;
    public Button resetButton;
    public Image cauldronImage;
    public TMP_Text feedbackText;
    private Animator animator;

    // Inspector references to cauldron
    public GameObject cauldron;
    public ParticleSystem sleepParticles;
    public GameObject goblinCharacter;
    public GameObject witchCharacter;



    void Start()
    {
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        if (defaultPotionMaterial != null)
            potionLiquidRenderer.material = defaultPotionMaterial;
        feedbackText.text = "Add 3 ingredients!";
        animator = GetComponent<Animator>();
        cauldron = GameObject.Find("Cauldron");
        // for color changing witch, get all her components
        witchRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        originalColors = new Color[witchRenderers.Length];
        for (int i = 0; i < witchRenderers.Length; i++)
        {
            witchRenderers[i].material = new Material(witchRenderers[i].sharedMaterial);
            originalColors[i] = witchRenderers[i].material.color;
        }
    }

    // for testing
    void Update()
    {
    if (Input.GetKeyDown(KeyCode.P))
       StartCoroutine(TurnPurple());
    if (Input.GetKeyDown(KeyCode.S))
        sleepParticles.Play();
    }

    public void OnIngredientClicked(string ingredient) {

        animator.SetTrigger("AddIngredient");

        if (ingredients.Count >= MAX_INGREDIENTS) {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Cauldron full, click stir";
            return;
        }

        if (ingredients.Contains(ingredient)) {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Add a different ingredient!";
            return;
        }

        // if not already in ingredients, add to ingredients
        ingredients.Add(ingredient);
        if (ingredients.Count == MAX_INGREDIENTS)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Click stir";
            stirButton.gameObject.SetActive(true);
            return;
        }
    }

    private Material GetPotionMaterial()
    {
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice"))
            return lovePotionMaterial;   // love potion
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool"))
            return shrinkingMaterial;  // shrinking/growing
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears"))
            return sleepingMaterial;   // sleeping
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice"))
            return explosionMaterial;  // explosion
        if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice"))
            return animalMaterial;  // turn into animal
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice"))
            return stoneMaterial;    // turn into stone
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice"))
            return levitationMaterial;   // levitation
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears"))
            return strengthMaterial;    // strength
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears"))
            return purpleMaterial;   // turn purple
        if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice"))
            return goblinMaterial;  // turn into goblin
        return null;
    }

    public void OnStirClicked()
    {
        animator.SetTrigger("Stir");
        Material mat = GetPotionMaterial();
        if (mat != null) {
            potionLiquidRenderer.material = mat;
        }
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(true);
    }

    public void OnDrinkClicked()
    {
        animator.SetTrigger("Drink");
        Debug.Log("ingredients: " + string.Join(", ", ingredients));
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // love potion reaction
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool")) {
            // shrinking/growing reaction
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // sleeping reaction
            sleepParticles.Play();
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice")) {
            // explosion reaction
        }
        else if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into animal reaction
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into stone reaction
            StartCoroutine(TurnToStone());
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // levitation reaction
            animator.SetTrigger("Levitate");
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // strength reaction
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears")) {
            // turn purple reaction
            // coroutine moves a little each frame for couple seconds
            StartCoroutine(TurnPurple());
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into goblin reaction
            animator.SetTrigger("Spin");
            Invoke("SwapToGoblin", 9.0f);
        }
        drinkButton.gameObject.SetActive(false);
    }

    public void OnResetClicked()
    {
        ingredients.Clear();
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "Add 3 ingredients!";
        animator.speed = 1f;
        StartCoroutine(ResetColor());
        if (defaultPotionMaterial != null) {
            potionLiquidRenderer.material = defaultPotionMaterial;
        }
        goblinCharacter.SetActive(false);
        animator = GetComponent<Animator>();
        witchCharacter.SetActive(true);
    }
    void SwapToGoblin() {
        witchCharacter.SetActive(false);
        goblinCharacter.SetActive(true);
        animator = goblinCharacter.GetComponent<Animator>();
        animator.Play("Spin", 0, 0.01f);
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

    private IEnumerator TurnToStone()
    {
        Color stone = new Color(0.3f, 0.27f, 0.25f);
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < witchRenderers.Length; i++)
                witchRenderers[i].material.color = Color.Lerp(originalColors[i], stone, t);
            animator.speed = Mathf.Lerp(1f, 0f, t); // slow to a stop
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

}
