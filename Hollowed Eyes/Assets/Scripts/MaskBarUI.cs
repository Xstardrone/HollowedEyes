using UnityEngine;
using UnityEngine.UI;

public class MaskBarUI : MonoBehaviour
{
    [Header("Container (required)")]
    public Transform maskContainer;
    
    [Header("Optional - leave null to auto-create slots")]
    public GameObject maskSlotPrefab;
    
    [Header("Slot Settings (used if auto-creating)")]
    public float slotSize = 80f;
    public float iconSize = 64f;
    public float lockSize = 40f;
    public float slotSpacing = 10f;

    MaskDatabase database;

    void Start()
    {
        LoadMasks();
        CreateMaskSlots();
    }

    void LoadMasks()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/masks");
        if (json == null) return;
        
        database = JsonUtility.FromJson<MaskDatabase>(json.text);
    }

    void CreateMaskSlots()
    {
        if (database == null || database.masks == null) return;
        
        if (maskContainer == null)
        {
            maskContainer = transform;
        }
        
        // Clear any existing children
        foreach (Transform child in maskContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Set up horizontal layout for centering
        HorizontalLayoutGroup layout = maskContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = maskContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        layout.spacing = slotSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        
        // Add ContentSizeFitter to auto-size the container
        ContentSizeFitter fitter = maskContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = maskContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        foreach (MaskData mask in database.masks)
        {
            GameObject slotObj;
            MaskSlotUI slot;
            
            if (maskSlotPrefab != null)
            {
                slotObj = Instantiate(maskSlotPrefab, maskContainer);
                slot = slotObj.GetComponent<MaskSlotUI>();
                if (slot == null)
                {
                    slot = slotObj.AddComponent<MaskSlotUI>();
                }
            }
            else
            {
                slotObj = new GameObject($"MaskSlot_{mask.maskName}");
                slotObj.transform.SetParent(maskContainer, false);
                slotObj.AddComponent<RectTransform>();
                slot = slotObj.AddComponent<MaskSlotUI>();
            }
            
            // Always apply size settings
            slot.slotSize = slotSize;
            slot.iconSize = iconSize;
            slot.lockSize = lockSize;

            slot.Setup(mask, mask.keybind);
        }
        
    }
}
