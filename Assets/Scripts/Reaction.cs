// each ingredient is a button
// exactly 3 ingredients must be added, prompt user to add more if < 3. must be different
// ingredients, so if user tries to click an ingredient already in set,
// have a pop up that says "choose a different ingredient"
// and once 3 are added, there is a button that pops up that says stir
// once its stirred, it will turn a unqiue color based on the type of potion.
// then once stirred a button will pop up to have the witch drink it.
// based on the type of potion (determined by which 3 ingredients), the witch will have a
// unique reaction

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Reactions : MonoBehaviour
{
    private HashSet<string> ingredients = new HashSet<string>();
    private const int MAX_INGREDIENTS = 3;
    private Color defaultCauldronColor = new Color(1f, 0.41f, 0.71f);
    public Button drinkButton;
    public Button stirButton;
    public Button resetButton;
    public Image cauldronImage;
    public TMP_Text feedbackText;
    private Animator animator;



    void Start()
    {
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        cauldronImage.color  = defaultCauldronColor;
        feedbackText.text = "Add 3 ingredients!";
        animator = GetComponent<Animator>();
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
            stirButton.gameObject.SetActive(true);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Click stir";
            return;

        }
    }

    public void OnStirClicked()
    {
        animator.SetTrigger("Stir");
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // love potion
            cauldronImage.color = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool")) {
            // shrinking/growing
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // sleeping
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice")) {
            // Explosion
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into animal
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into stone and topple over
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // levitation
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // strength
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears")) {
            // turn purple
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into goblin
            cauldronImage.color  = new Color(1f, 0.41f, 0.71f);
        }
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(true);
    }

    public void OnDrinkClicked()
    {
        animator.SetTrigger("Drink");
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // love potion

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool")) {
            // shrinking/growing

        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // sleeping

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice")) {
            // Explosion

        }
        else if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into animal

        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into stone and topple over

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // levitation

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // strength

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears")) {
            // turn purple

        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into goblin

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
            cauldronImage.color  = defaultCauldronColor;
    }
}