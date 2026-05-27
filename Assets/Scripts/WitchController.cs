using UnityEngine;

public class WitchController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAddIngredient()
    {
        animator.SetTrigger("AddIngredient");
    }

    public void PlayStir()
    {
        animator.SetTrigger("Stir");
    }

    public void PlayDrink()
    {
        animator.SetTrigger("Drink");
    }
}
