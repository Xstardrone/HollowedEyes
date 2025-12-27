using UnityEngine;

public class PlayerMaskController : MonoBehaviour
{
    public static PlayerMaskController Instance;
    
    public int activeMask;

    MaskDatabase database;
    int lastKnownLevel = -1;
    bool isReady = false;

    void Awake()
    {

        Instance = this;
    }

    void Start()
    {
        LoadMasks();
        activeMask = 1;
        Debug.Log("PlayerMaskController ready - press 1/2/3/4 to switch masks");
    }

    void LoadMasks()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/masks");
        if (json == null)
        {
            Debug.LogError("Failed to load masks.json!");
            return;
        }
        
        database = JsonUtility.FromJson<MaskDatabase>(json.text);
        if (database == null || database.masks == null)
        {
            Debug.LogError("Failed to parse masks!");
            return;
        }
        isReady = true;
    }

    void Update()
    {
        if (!isReady) return;
        
        // Check for level changes
        CheckLevelChange();
        
        // Handle input - log ANY key to verify input is working
        if (Input.anyKeyDown)
        {
            Debug.Log($"Key pressed: {Input.inputString}");
        }
        
        HandleMaskSwitch();
        HandleAbility();
    }
    
    void CheckLevelChange()
    {
        if (LevelGetter.Instance == null)
        {
            Debug.LogWarning("LevelGetter.Instance is null!");
            return;
        }
        
        int currentLevel = LevelGetter.Instance.CurrentLevel;
        if (currentLevel != lastKnownLevel)
        {
            Debug.Log($"Level changed from {lastKnownLevel} to {currentLevel}");
            lastKnownLevel = currentLevel;
            // Notify UI to refresh lock states
            RefreshAllMaskSlots();
        }
    }
    
    void RefreshAllMaskSlots()
    {
        // Find all MaskSlotUI and refresh them
        MaskSlotUI[] slots = FindObjectsOfType<MaskSlotUI>();
        Debug.Log($"Found {slots.Length} mask slots to refresh");
        foreach (var slot in slots)
        {
            slot.RefreshState();
        }
    }

    void HandleMaskSwitch()
    {
        // Check number keys (both alpha and numpad)
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("Key 1 detected!");
            TrySwitch(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            Debug.Log("Key 2 detected!");
            TrySwitch(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            Debug.Log("Key 3 detected!");
            TrySwitch(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            Debug.Log("Key 4 detected!");
            TrySwitch(4);
        }
    }

    void TrySwitch(int maskNumber)
    {
        if (LevelGetter.Instance == null)
        {
            Debug.LogError("LevelGetter not found!");
            return;
        }
        
        // Find the mask data
        MaskData mask = null;
        foreach (MaskData m in database.masks)
        {
            if (m.maskNumber == maskNumber)
            {
                mask = m;
                break;
            }
        }
        
        if (mask == null)
        {
            Debug.LogError($"Mask {maskNumber} not found in database!");
            return;
        }
        
        if (LevelGetter.Instance.CurrentLevel < mask.unlockLevel)
        {
            Debug.Log($"{mask.maskName} mask is locked");
            return;
        }

        activeMask = mask.maskNumber;
        Debug.Log($"Switched to {mask.maskName} mask");
    }

    void HandleAbility()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        
        switch (activeMask)
        {
            case 2:
                UseTrap();
                break;
            case 3:
                UseRope();
                break;
            case 4:
                UsePhase();
                break;
        }
    }

    void UseTrap()
    {
        // will do
    }
    
    void UseRope()
    {
        // rishiiiii
    }
    
    void UsePhase()
    {
        //need to implement
    }
}
