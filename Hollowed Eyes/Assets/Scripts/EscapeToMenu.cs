using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EscapeToMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Main Menu";
    
    void Update()
    {
        // Check for Escape key press
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GoToMainMenu();
        }
    }
    
    void GoToMainMenu()
    {
        // Reset time scale in case it was changed (like from time slow ability)
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(menuSceneName);
    }
}