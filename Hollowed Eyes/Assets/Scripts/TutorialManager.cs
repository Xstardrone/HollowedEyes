using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialMessagePanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    
    private string[] tutorialMessages = new string[]
    {
        "Welcome to Hollowed Eyes. Press SPACE to continue.",
        "Your goal is to reach the exit using different masks",
        "Different masks will provide you with different abilities.",
        "For example, the first mask you start with, allows for double jump.",
        "Click 'E' to use the other mask's special ability.",
        "Use WAD (or arrow keys) to jump and move left/right. ",
        "Press 1, 2, 3, or 4 to switch between masks.",
        "Each mask has limited uses per level, as you can see in the bottom right of each mask slot.",
        "Good luck adventurer!"
    };
    
    private int currentMessageIndex = 0;
    private bool tutorialActive = true;

    void Start()
    {
        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(true);
        }
        
        ShowCurrentMessage();
    }

    void Update()
    {
        if (!tutorialActive) return;
        
        // Check for SPACE key press
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextMessage();
        }
    }
    
    void ShowCurrentMessage()
    {
        if (tutorialText != null && currentMessageIndex < tutorialMessages.Length)
        {
            tutorialText.text = tutorialMessages[currentMessageIndex];
        }
    }
    
    void NextMessage()
    {
        currentMessageIndex++;
        
        if (currentMessageIndex >= tutorialMessages.Length)
        {
            // Tutorial finished
            EndTutorial();
        }
        else
        {
            // Show next message
            ShowCurrentMessage();
        }
    }
    
    void EndTutorial()
    {
        tutorialActive = false;
        
        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }
        
        Debug.Log("Tutorial completed!");
    }
}