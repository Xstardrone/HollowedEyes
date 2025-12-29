using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class PlayerMaskController : MonoBehaviour
{
    public static PlayerMaskController Instance;
    
    public int activeMask = 1;
    [SerializeField] public Animator anim;
    [SerializeField] public Renderer maskSwap;

    // Mask uses remaining for current level
    public Dictionary<int, int> maskUses = new Dictionary<int, int>();
    
    // Hardcoded uses per level: maskUsesPerLevel[level][maskNumber] = uses
    public static Dictionary<int, Dictionary<int, int>> maskUsesPerLevel = new Dictionary<int, Dictionary<int, int>>
    {
        { 1, new Dictionary<int, int> { {1, 3}, {2, 0}, {3, 0}, {4, 0} } },
        { 2, new Dictionary<int, int> { {1, 8}, {2, 4}, {3, 0}, {4, 0} } },
        { 3, new Dictionary<int, int> { {1, 4}, {2, 1}, {3, 7}, {4, 0} } },
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
    public bool isPhasing = false;
    float phaseEndTime = 0f;
    float phaseCooldown = 0f;
    float maxPhaseCooldown = 5f;
    int lastDisplayedCooldown = 0;
    Collider2D playerCollider;

    public GameObject swingNode;
    private GameObject ropeRenderer;
    private bool isRopeActive = false;
    
    [Header("Rope Settings")]
    [SerializeField] private float maxRopeDistance = 15f;


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
                if (activeMask == 3 && isRopeActive)
                {
                    CutRope();
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
        setMaskColor();
        showObject();
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
    
    public void ResetAllUses()
    {
        if (LevelGetter.Instance == null) return;
        
        int currentLevel = LevelGetter.Instance.CurrentLevel;
        LoadUsesForLevel(currentLevel);
        
        // Also reset Phase cooldown
        phaseCooldown = 0f;
        
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
            setMaskColor();
            showObject();
            anim.SetInteger("activeMask", activeMask);
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
        
        // Cut rope if active when switching masks
        if (isRopeActive && activeMask == 3)
        {
            CutRope();
        }

        activeMask = mask.maskNumber;
        setMaskColor();
        showObject();
    }

    void setMaskColor()
    {
        if (activeMask == 0)
        {
            maskSwap.material.SetColor("_ToRed", new Color32(0xbd, 0xb1, 0xb1, 0xff));
            maskSwap.material.SetColor("_ToGreen", new Color32(0xbd, 0xb1, 0xb1, 0xff));
            maskSwap.material.SetColor("_ToBlue", new Color32(0xbd, 0xb1, 0xb1, 0xff));
        } else if (activeMask == 1)
        {
            Debug.Log("Venetian");
            maskSwap.material.SetColor("_ToRed", new Color32(0xff, 0xff, 0xff, 0xff));
            maskSwap.material.SetColor("_ToGreen", new Color32(0xd0, 0xd0, 0xd0, 0xff));
            maskSwap.material.SetColor("_ToBlue", new Color32(0xff, 0xff, 0xff, 0xff));
        } else if (activeMask == 2)
        {
            Debug.Log("Oni");
            maskSwap.material.SetColor("_ToRed", new Color32(0xba, 0x1b, 0x1b, 0xff));
            maskSwap.material.SetColor("_ToGreen", new Color32(0x80, 0x12, 0x12, 0xff));
            maskSwap.material.SetColor("_ToBlue", new Color32(0xba, 0x1b, 0x1b, 0xff));
        } else if (activeMask == 3)
        {
            Debug.Log("Rope");
            maskSwap.material.SetColor("_ToRed", new Color32(0xae, 0x85, 0x52, 0xff));
            maskSwap.material.SetColor("_ToGreen", new Color32(0x90, 0x6d, 0x43, 0xff));
            maskSwap.material.SetColor("_ToBlue", new Color32(0xae, 0x85, 0x52, 0xff));
        } else if (activeMask == 4)
        {
            Debug.Log("tron");
            maskSwap.material.SetColor("_ToRed", new Color32(0xff, 0x9d, 0x00, 0xff));
            maskSwap.material.SetColor("_ToGreen", new Color32(0x00, 0x00, 0x00, 0xff));
            maskSwap.material.SetColor("_ToBlue", new Color32(0x16, 0x16, 0x16, 0xff));
        }
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
        int uses = GetUsesForMask(3);
        if (uses <= 0)
        {
            Debug.Log($"Rope failed: No uses left (uses={uses})");
            return;
        }
        
        GameObject nearestNode = FindNearestNode();
        if (nearestNode == null)
        {
            Debug.Log("Rope failed: No node found in range");
            return;
        }
        
        Debug.Log($"Attempting rope to node: {nearestNode.name} at distance {Vector2.Distance(transform.position, nearestNode.transform.position)}");
        
        if (UseOneMask(3))
        {
            swingNode = nearestNode;
            SpringJoint2D spring = swingNode.GetComponent<SpringJoint2D>();
            Debug.Log($"Rope activated! Uses remaining: {GetUsesForMask(3)}");
            
            // Configure spring joint for better swing physics
            float distance = Vector2.Distance(transform.position, swingNode.transform.position);
            spring.distance = distance; // Lock to activation radius
            spring.dampingRatio = 0.7f; // Reduce momentum (higher = less bouncy)
            spring.frequency = 1f; // Make rope stiffer (lower = more slack)
            spring.autoConfigureDistance = false; // Don't pull inward
            
            spring.enabled = true;
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            ropeRenderer.GetComponent<RopeRendering>().enabled = true;
            isRopeActive = true;
            anim.SetBool("Rope", true);
        }
    }
    
    void CutRope()
    {
        if (!isRopeActive) return;
        
        // Store current velocity before cutting and boost it
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 currentVelocity = rb.linearVelocity * 1.2f; // 20% more momentum
        
        // Disable rope
        if (swingNode != null)
        {
            swingNode.GetComponent<SpringJoint2D>().enabled = false;
        }
        GetComponent<ReactivateMovement>().enabled = true;
        ropeRenderer.GetComponent<RopeRendering>().enabled = false;
        
        // Apply reduced velocity
        rb.linearVelocity = currentVelocity;
        
        isRopeActive = false;
        anim.SetBool("Rope", false);
        
        // Reset air jump ONLY after cutting rope (not on normal mask switches)
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ResetAirJump();
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
            if (dist < minDist && dist <= maxRopeDistance)
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
        
        // Also ignore collision with PhaseTrap objects during phase
        GameObject[] phaseTrapObjects = GameObject.FindGameObjectsWithTag("PhaseTrap");
        foreach (GameObject obj in phaseTrapObjects)
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
        
        // Re-enable collision with PhaseTrap objects
        GameObject[] phaseTrapObjects = GameObject.FindGameObjectsWithTag("PhaseTrap");
        foreach (GameObject obj in phaseTrapObjects)
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

    void showObject()
    {
        int disabledCount = 0;
        int enabledCount = 0;
        
        // Get all colliders to handle child colliders too
        Dictionary<GameObject, bool> processedRoots = new Dictionary<GameObject, bool>();
        
        foreach (Transform platform in GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            SpriteRenderer spriteRenderer = platform.gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                continue;
            }
            
            string objTag = platform.gameObject.tag;
            
            // Get ALL colliders including children
            Collider2D[] allColliders = platform.gameObject.GetComponentsInChildren<Collider2D>(true);
            Collider2D objCollider = platform.gameObject.GetComponent<Collider2D>();
            
            // Special handling for Trap tag - always collidable, only visible with Mask 2
            if (objTag == "Trap")
            {
                spriteRenderer.enabled = (activeMask == 2);
                // Keep collider always enabled for traps
                if (objCollider != null)
                {
                    objCollider.enabled = true;
                }
                continue;
            }
            
            // Special handling for PhaseTrap tag - visible with Trap (2) or Phase (4) mask
            if (objTag == "PhaseTrap")
            {
                spriteRenderer.enabled = (activeMask == 2 || activeMask == 4);
                // Disable child sprite renderers too
                foreach (SpriteRenderer childRenderer in platform.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    childRenderer.enabled = (activeMask == 2 || activeMask == 4);
                }
                // Keep collider always enabled (will be ignored during phase mode)
                if (objCollider != null)
                {
                    objCollider.enabled = true;
                }
                continue;
            }
            
            // Objects without mask-specific tags are always visible
            bool isMaskSpecific = (objTag == "Mask 1" || objTag == "Mask 2" || objTag == "Mask 3" || objTag == "Mask 4");
            
            bool shouldBeVisible = false;
            
            if (!isMaskSpecific)
            {
                // Always show non-mask-specific objects
                shouldBeVisible = true;
            }
            else
            {
                // Show mask-specific objects only when their mask is active
                switch (activeMask)
                {
                    case 1:
                        shouldBeVisible = (objTag == "Mask 1");
                        break;
                    case 2:
                        shouldBeVisible = (objTag == "Mask 2");
                        break;
                    case 3:
                        shouldBeVisible = (objTag == "Mask 3");
                        break;
                    case 4:
                        shouldBeVisible = (objTag == "Mask 4");
                        break;
                }
            }
            
            spriteRenderer.enabled = shouldBeVisible;
            
            // Disable child sprite renderers too
            foreach (SpriteRenderer childRenderer in platform.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
            {
                childRenderer.enabled = shouldBeVisible;
            }
            
            // Always set collision state based on visibility - for ALL colliders including children
            if (allColliders.Length > 0 && playerCollider != null)
            {
                foreach (Collider2D col in allColliders)
                {
                    if (col == null) continue;
                    
                    if (!shouldBeVisible)
                    {
                        // Make invisible objects not collide with player
                        col.enabled = false;
                        Physics2D.IgnoreCollision(col, playerCollider, true);
                    }
                    else
                    {
                        // Make visible objects collide with player
                        Physics2D.IgnoreCollision(col, playerCollider, false);
                        col.enabled = true;
                    }
                }
                
                if (!shouldBeVisible)
                {
                    // Debug.Log($"Ignoring collision with {platform.gameObject.name} ({allColliders.Length} colliders) (tag: {objTag})");
                    disabledCount++;
                }
                else
                {
                    // Debug.Log($"Restoring collision with {platform.gameObject.name} ({allColliders.Length} colliders) (tag: {objTag})");
                    enabledCount++;
                }
            }
        }
        
        Debug.Log($"ShowObject complete - Active Mask: {activeMask}, Disabled: {disabledCount}, Enabled: {enabledCount}");
    }
}