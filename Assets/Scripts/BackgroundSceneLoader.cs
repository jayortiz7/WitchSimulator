using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundSceneLoader : MonoBehaviour
{
    void Start()
    {
        if (!SceneManager.GetSceneByName("WitchBackground").isLoaded)
            SceneManager.LoadScene("WitchBackground", LoadSceneMode.Additive);
    }
}
