using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class SceneShortcut
{
    public string sceneName;
    public Key hotkey;
}

public class SceneShortcutManager : MonoBehaviour
{
    [Header("Scene Shortcuts (Hold Shift + Key)")]
    public List<SceneShortcut> shortcuts = new List<SceneShortcut>
    {
        new SceneShortcut { sceneName = "Level 1", hotkey = Key.Digit1 },
        new SceneShortcut { sceneName = "Level 2", hotkey = Key.Digit2 },
        new SceneShortcut { sceneName = "Level 3", hotkey = Key.Digit3 },
        new SceneShortcut { sceneName = "Level 4", hotkey = Key.Digit4 },
        new SceneShortcut { sceneName = "Main Menu", hotkey = Key.Digit5 },
        new SceneShortcut { sceneName = "End Menu", hotkey = Key.Digit6 }
    };

    void Update()
    {
        if (Keyboard.current == null) return;
        
        // Check if Shift is held down
        bool shiftHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        
        if (!shiftHeld) return;
        
        // Check each shortcut
        foreach (SceneShortcut shortcut in shortcuts)
        {
            if (Keyboard.current[shortcut.hotkey].wasPressedThisFrame)
            {
                LoadScene(shortcut.sceneName);
                break;
            }
        }
    }
    
    void LoadScene(string sceneName)
    {
        // Reset time scale in case it was changed
        Time.timeScale = 1f;
        
        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}