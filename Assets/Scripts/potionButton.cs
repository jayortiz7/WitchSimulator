using UnityEngine;

public class PotionClick : MonoBehaviour
{
    public Reactions witch;
    public string potionName;

    private void OnMouseDown()
    {
        Debug.Log("Clicked " + potionName);

        witch.OnIngredientClicked(potionName);
    }
}