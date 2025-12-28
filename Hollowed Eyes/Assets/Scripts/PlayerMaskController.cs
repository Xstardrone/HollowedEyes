using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerMaskController : MonoBehaviour
{
    public static PlayerMaskController Instance;
    
    public int activeMask = 1;
    
    // Mask uses remaining for current level
    public Dictionary<int, int> maskUses = new Dictionary<int, int>();
    
    // Hardcoded uses per level: maskUsesPerLevel[level][maskNumber] = uses
    public static Dictionary<int, Dictionary<int, int>> maskUsesPerLevel = new Dictionary<int, Dictionary<int, int>>
    {
        { 1, new Dictionary<int, int> { {1, 3}, {2, 0}, {3, 0}, {4, 0} } },
        { 2, new Dictionary<int, int> { {1, 2}, {2, 1}, {3, 0}, {4, 0} } },
        { 3, new Dictionary<int, int> { {1, 2}, {2, 2}, {3, 1}, {4, 0} } },
        { 4, new Dictionary<int, int> { {1, 2}, {2, 2}, {3, 2}, {4, 5} } },
        { 5, new Dictionary<int, int> { {1, 3}, {2, 2}, {3, 2}, {4, 5} } },
    };

    MaskDatabase database;
    int lastKnownLevel = -1;
    bool isReady = false;
    
    // Trap ability (time slow)
    bool isTimeSlowed = false;
    float timeSlowEndTime = 0f;
    float originalTimeScale = 1f;
    float originalFixedDeltaTime;
    
    // Phase ability
    bool isPhasing = false;
    float phaseEndTime = 0f;
    float phaseCooldown = 0f;
    float maxPhaseCooldown = 5f;
    int lastDisplayedCooldown = 0;
    Collider2D playerCollider;

    public GameObject swingNode;
    private GameObject ropeRenderer;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        LoadMasks();
        InitializeForLevel();
        playerCollider = GetComponent<Collider2D>();
        ropeRenderer = GameObject.Find("RopeRenderer");
    }

    void Update()
    {
        
        if (!isReady)
        {
            Debug.Log("Not ready - database not loaded");
            return;
        }

        // NEW Input System for mask switching
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                TrySwitch(1);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                TrySwitch(2);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                TrySwitch(3);
            }
            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                TrySwitch(4);
            }
            
            // E key for ability with NEW Input System
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("E key pressed! Active mask: " + activeMask);
                HandleAbility();
            }
            if (Keyboard.current.eKey.wasReleasedThisFrame)
            {
                if (activeMask == 3)
                {
                    swingNode.GetComponent<SpringJoint2D>().enabled = false;
                    // gameObject.GetComponent<PlayerMovement>().enabled = true;
                    GetComponent<ReactivateMovement>().enabled = true;
                    ropeRenderer.GetComponent<RopeRendering>().enabled = false;

                }
            }
        }

        CheckLevelChange();
        UpdateTimeSlow();
        UpdatePhase();
        UpdatePhaseCooldown();
    }
    
    void InitializeForLevel()
    {
        if (LevelGetter.Instance == null)
        {
            return;
        }
        
        int level = LevelGetter.Instance.CurrentLevel;
        activeMask = Mathf.Clamp(level, 1, 4);
        LoadUsesForLevel(level);
        lastKnownLevel = level;
    }
    
    void LoadUsesForLevel(int level)
    {
        maskUses.Clear();
        
        if (maskUsesPerLevel.ContainsKey(level))
        {
            foreach (var pair in maskUsesPerLevel[level])
            {
                maskUses[pair.Key] = pair.Value;
            }
        }
        else
        {
            for (int i = 1; i <= 4; i++)
            {
                maskUses[i] = (i <= level) ? 2 : 0;
            }
        }
        
        RefreshAllMaskSlots();
    }
    
    public int GetUsesForMask(int maskNumber)
    {
        // Phase mask (4) uses cooldown system instead of uses
        if (maskNumber == 4)
        {
            return Mathf.CeilToInt(phaseCooldown);
        }
        return maskUses.ContainsKey(maskNumber) ? maskUses[maskNumber] : 0;
    }
    
    public bool UseOneMask(int maskNumber)
    {
        if (maskUses.ContainsKey(maskNumber) && maskUses[maskNumber] > 0)
        {
            maskUses[maskNumber]--;
            RefreshAllMaskSlots();
            return true;
        }
        return false;
    }
    
    // Check if bonus jump is available (has uses left for Basic mask)
    public bool CanUseBonusJump()
    {
        return activeMask == 1 && GetUsesForMask(1) > 0;
    }
    
    // Consume a use when bonus jump is used
    public bool UseBonusJump()
    {
        if (activeMask == 1)
        {
            return UseOneMask(1);
        }
        return false;
    }
    
    // Returns bonus jumps if Basic mask is equipped AND has uses
    public int GetBonusJumps()
    {
        if (activeMask == 1 && GetUsesForMask(1) > 0)
        {
            return 1;
        }
        return 0;
    }

    void LoadMasks()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/masks");
        if (json == null)
        {
            Debug.LogError("Could not load masks.json from Resources/Data/");
            return;
        }
        
        database = JsonUtility.FromJson<MaskDatabase>(json.text);
        if (database == null || database.masks == null)
        {
            Debug.LogError("Failed to parse masks database");
            return;
        }
        
        isReady = true;
    }

    void UpdateTimeSlow()
    {
        if (isTimeSlowed && Time.unscaledTime >= timeSlowEndTime)
        {
            EndTimeSlow();
        }
    }
    
    void CheckLevelChange()
    {
        if (LevelGetter.Instance == null) return;
        
        int currentLevel = LevelGetter.Instance.CurrentLevel;
        if (currentLevel != lastKnownLevel)
        {
            lastKnownLevel = currentLevel;
            activeMask = Mathf.Clamp(currentLevel, 1, 4);
            LoadUsesForLevel(currentLevel);
        }
    }
    
    void RefreshAllMaskSlots()
    {
        MaskSlotUI[] slots = FindObjectsOfType<MaskSlotUI>();
        foreach (var slot in slots)
        {
            slot.RefreshState();
        }
    }

    void TrySwitch(int maskNumber)
    {
        
        if (LevelGetter.Instance == null)
        {
            return;
        }
        
        if (database == null || database.masks == null)
        {
            return;
        }
        
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
            return;
        }
        
        
        if (LevelGetter.Instance.CurrentLevel < mask.unlockLevel)
        {
            return;
        }

        activeMask = mask.maskNumber;
    }

    void HandleAbility()
    {
        
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
            default:
                break;
        }
    }

    void UseTrap()
    {
        
        if (isTimeSlowed)
        {
            return;
        }
        if (GetUsesForMask(2) <= 0)
        {
            return;
        }
        
        if (UseOneMask(2))
        {
            StartTimeSlow();
        }
    }
    
    void StartTimeSlow()
    {
        isTimeSlowed = true;
        originalTimeScale = Time.timeScale;
        
        Time.timeScale = originalTimeScale / 3f;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
        
        timeSlowEndTime = Time.unscaledTime + 1f;
    }
    
    void EndTimeSlow()
    {
        isTimeSlowed = false;
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
    
    void OnDestroy()
    {
        if (isTimeSlowed)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
        
        if (isPhasing)
        {
            EndPhase();
        }
    }
    
    void UseRope()
    {
        if (GetUsesForMask(3) <= 0)
        {
            return;
        }
        else if (FindNearestNode() == null)
        {
            return;
        }
        else if (UseOneMask(3))
        {
            swingNode = FindNearestNode();
            swingNode.GetComponent<SpringJoint2D>().enabled = true;
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            ropeRenderer.GetComponent<RopeRendering>().enabled = true;
        }
    }

    GameObject FindNearestNode()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        GameObject nearestNode = null;
        float minDist = Mathf.Infinity;
        Vector2 playerPos = transform.position;
        string facing = GetComponent<PlayerMovement>().GetFacing();
        GameObject[] facingNodes = Array.FindAll(nodes, node =>
        {
            if (facing == "right" && node.transform.position.x >= playerPos.x)
            {
                return true;
            }
            else if (facing == "left" && node.transform.position.x <= playerPos.x)
            {
                return true;
            }
            return false;
        });
        foreach (GameObject node in facingNodes)
        {
            float dist = Vector2.Distance(playerPos, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestNode = node;
            }
        }
        return nearestNode;
    }
    
    void UsePhase()
    {
        if (isPhasing) return;
        if (phaseCooldown > 0f) return;
        
        StartPhase();
    }
    
    void StartPhase()
    {
        isPhasing = true;
        phaseEndTime = Time.time + 1f;
        
        // Find all objects with "Phasable" tag and ignore collision
        GameObject[] phasableObjects = GameObject.FindGameObjectsWithTag("Phasable");
        foreach (GameObject obj in phasableObjects)
        {
            Collider2D objCollider = obj.GetComponent<Collider2D>();
            if (objCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, objCollider, true);
            }
        }
    }
    
    void UpdatePhase()
    {
        if (isPhasing && Time.time >= phaseEndTime)
        {
            EndPhase();
        }
    }
    
    void EndPhase()
    {
        isPhasing = false;
        phaseCooldown = maxPhaseCooldown;
        lastDisplayedCooldown = Mathf.CeilToInt(phaseCooldown);
        
        // Re-enable collision with all Phasable objects
        GameObject[] phasableObjects = GameObject.FindGameObjectsWithTag("Phasable");
        foreach (GameObject obj in phasableObjects)
        {
            Collider2D objCollider = obj.GetComponent<Collider2D>();
            if (objCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, objCollider, false);
            }
        }
        
        RefreshAllMaskSlots();
    }
    
    void UpdatePhaseCooldown()
    {
        if (phaseCooldown > 0f)
        {
            phaseCooldown -= Time.deltaTime;
            
            // Check if the displayed integer value has changed
            int currentDisplayed = Mathf.CeilToInt(phaseCooldown);
            if (currentDisplayed != lastDisplayedCooldown)
            {
                lastDisplayedCooldown = currentDisplayed;
                RefreshAllMaskSlots();
            }
            
            if (phaseCooldown <= 0f)
            {
                phaseCooldown = 0f;
                RefreshAllMaskSlots();
            }
        }
    }
}