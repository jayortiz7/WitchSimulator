// each ingredient is a button
// exactly 3 ingredients must be added, prompt user to add more if < 3. must be different
// ingredients, so if user tries to click an ingredient already in set, 
// have a pop up that says "choose a different ingredient"
// and once 3 are added, there is a button that pops up that says stir
// once its stirred, it will turn a unqiue color based on the type of potion.
// then once stirred a button will pop up to have the witch drink it.
// based on the type of potion (determined by which 3 ingredients), the witch will have a 
// unique reaction
// use sprites for texture/color rather than just potionLiquidImage.sprite (do .sprite instead)
// and declare each field like LovePotionSprite then link the sprite in unity to each field
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reactions : MonoBehaviour
{
    private HashSet<string> ingredients = new HashSet<string>();
    private const int MAX_INGREDIENTS = 3;

    public Sprite lovePotionSprite;
    public Sprite shrinkingSprite;
    public Sprite sleepingSprite;
    public Sprite explosionSprite;
    public Sprite animalSprite;
    public Sprite stoneSprite;
    public Sprite levitationSprite;
    public Sprite strengthSprite;
    public Sprite purpleSprite;
    public Sprite goblinSprite;
    private Color defaultPotionColor = new Color(1f, 0.41f, 0.71f);
    public Button drinkButton;
    public Button stirButton;
    public Button resetButton;
    public Image potionLiquidImage;
    public Text feedbackText;
    


    void Start()
    {
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        potionLiquidImage.sprite  = defaultPotionColor;
        feedbackText.text = "Add 3 ingredients!";
    }

    public void OnIngredientClicked(string ingredient) {
        
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

    public void OnStirClicked()
    {
        if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // love potion
            potionLiquidImage.sprite = lovePotionSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool")) {
            // shrinking/growing
            potionLiquidImage.sprite = shrinkingSprite;
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // sleeping
            potionLiquidImage.sprite = sleepingSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Black Magic Bean Juice")) {
            // Explosion
            potionLiquidImage.sprite = explosionSprite;
        }
        else if (ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into animal
            potionLiquidImage.sprite = animalSprite;
        }
        else if (ingredients.Contains("Dragon's Blood") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into stone and topple over
            potionLiquidImage.sprite = stoneSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Black Magic Bean Juice")) {
            // levitation
            potionLiquidImage.sprite = levitationSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Bat Drool") && ingredients.Contains("Unicorn Tears")) {
            // strength
            potionLiquidImage.sprite = strengthSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Dragon's Blood") && ingredients.Contains("Unicorn Tears")) {
            // turn purple
            potionLiquidImage.sprite = purpleSprite;
        }
        else if (ingredients.Contains("Goblin Sweat") && ingredients.Contains("Unicorn Tears") && ingredients.Contains("Black Magic Bean Juice")) {
            // turn into goblin
            potionLiquidImage.sprite = goblinSprite;
        }
        stirButton.gameObject.SetActive(false);
        drinkButton.gameObject.SetActive(true);
    }

    public void OnDrinkClicked()
    {
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
            potionLiquidImage.sprite  = defaultPotionColor;
    }
}