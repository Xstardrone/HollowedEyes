using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class MaskMessagePanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject messagePanel;
    public TMP_Text maskText;
    public Image maskImage;
    
    private bool isShowingMessage = false;
    private int lastShownMaskLevel = -1;
    private int lastCheckedLevel = -1;

    void Start()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Check for level changes
        if (LevelGetter.Instance != null)
        {
            int currentLevel = LevelGetter.Instance.CurrentLevel;
            if (currentLevel != lastCheckedLevel)
            {
                lastCheckedLevel = currentLevel;
                CheckForMaskUnlock();
            }
        }
        
        // Check for SPACE to close message
        if (isShowingMessage && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HideMessage();
        }
    }

    void CheckForMaskUnlock()
    {
        if (LevelGetter.Instance == null) return;
        
        int currentLevel = LevelGetter.Instance.CurrentLevel;
        
        Debug.Log("Checking mask unlock for level: " + currentLevel);
        
        // Skip level 1 (Basic mask shown in tutorial)
        if (currentLevel <= 1)
        {
            Debug.Log("Level 1 or below, skipping mask message");
            return;
        }
        
        // Don't show same level twice
        if (lastShownMaskLevel == currentLevel)
        {
            Debug.Log("Already shown message for level " + currentLevel);
            return;
        }
        
        // Load mask database
        TextAsset json = Resources.Load<TextAsset>("Data/masks");
        if (json == null)
        {
            Debug.LogError("Could not load masks.json");
            return;
        }
        
        MaskDatabase database = JsonUtility.FromJson<MaskDatabase>(json.text);
        if (database == null || database.masks == null)
        {
            Debug.LogError("Failed to parse mask database");
            return;
        }
        
        // Find mask that unlocks at this level
        foreach (MaskData mask in database.masks)
        {
            Debug.Log("Checking mask " + mask.maskNumber + " - unlocks at level " + mask.unlockLevel);
            
            if (mask.unlockLevel == currentLevel)
            {
                Debug.Log("Found mask to unlock: " + mask.maskName);
                ShowMaskMessage(mask);
                lastShownMaskLevel = currentLevel;
                break;
            }
        }
    }

    void ShowMaskMessage(MaskData mask)
    {
        if (messagePanel == null)
        {
            Debug.LogError("Message panel is null!");
            return;
        }
        
        if (maskText == null)
        {
            Debug.LogError("Mask text is null!");
            return;
        }
        
        if (maskImage == null)
        {
            Debug.LogError("Mask image is null!");
            return;
        }
        
        Debug.Log("Showing mask message: " + mask.unlockMessage);
        
        // Set text
        maskText.text = mask.unlockMessage;
        
        // Load and set icon
        Sprite icon = Resources.Load<Sprite>(mask.iconPath);
        if (icon != null)
        {
            maskImage.sprite = icon;
            Debug.Log("Loaded icon from: " + mask.iconPath);
        }
        else
        {
            Debug.LogError("Could not load icon from: " + mask.iconPath);
        }
        
        // Show panel
        messagePanel.SetActive(true);
        isShowingMessage = true;
        
        Debug.Log("Panel should now be visible");
    }

    void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        isShowingMessage = false;
        Debug.Log("Message panel hidden");
    }
}