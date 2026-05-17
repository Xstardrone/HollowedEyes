using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLevel1Hard()
    {
        SceneManager.LoadScene("Level 1 HARD");
    }

    public void LoadLevel1Medium()
    {
        SceneManager.LoadScene("Level 1 Medium");
    }

    public void LoadLevel1Easy()
    {
        SceneManager.LoadScene("Level 1 Easy");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
