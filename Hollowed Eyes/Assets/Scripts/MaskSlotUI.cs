using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskSlotUI : MonoBehaviour
{
    [Header("Size Settings")]
    public float slotSize = 80f;      // Size of the background box
    public float iconSize = 64f;      // Size of the mask icon (independent of slot)
    public float lockSize = 40f;      // Size of the lock overlay
    public float keybindFontSize = 18f;
    
    [Header("References (Auto-created if null)")]
    public Image border;
    public Image icon;
    public Image lockOverlay;
    public TMP_Text keybindText;

    MaskData data;

    public void Setup(MaskData maskData, string keybind)
    {
        data = maskData;
        CreateUIElements();
        
        // Load and set icon sprite
        Sprite iconSprite = Resources.Load<Sprite>(maskData.iconPath);
        if (iconSprite != null)
        {
            icon.sprite = iconSprite;
        }
        
        // Load and set lock sprite
        if (!string.IsNullOrEmpty(maskData.lockPath))
        {
            Sprite lockSprite = Resources.Load<Sprite>(maskData.lockPath);
            if (lockSprite != null)
            {
                lockOverlay.sprite = lockSprite;
            }
        }
        
        // Set keybind text
        keybindText.text = keybind;
        
        // Apply mask's UI color to border if available
        if (!string.IsNullOrEmpty(maskData.uiColor))
        {
            if (ColorUtility.TryParseHtmlString(maskData.uiColor, out Color color))
            {
                border.color = color;
            }
        }

        UpdateState();
    }

    void CreateUIElements()
    {
        // Set up this slot's RectTransform
        RectTransform slotRect = GetComponent<RectTransform>();
        if (slotRect == null) slotRect = gameObject.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);
        
        // 1. Create or get Border (background)
        if (border == null)
        {
            border = CreateChildImage("Border", slotRect);
            border.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
        SetStretchFill(border.rectTransform);
        border.transform.SetAsFirstSibling();
        
        // 2. Create or get Icon
        if (icon == null)
        {
            icon = CreateChildImage("Icon", slotRect);
            icon.color = Color.white;
        }
        // ALWAYS apply size
        icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        icon.rectTransform.anchoredPosition = Vector2.zero;
        CenterAnchor(icon.rectTransform);
        
        // 3. Create or get Lock Overlay
        if (lockOverlay == null)
        {
            lockOverlay = CreateChildImage("Lock Overlay", slotRect);
            lockOverlay.color = Color.white;
        }
        // ALWAYS apply size
        lockOverlay.rectTransform.sizeDelta = new Vector2(lockSize, lockSize);
        lockOverlay.rectTransform.anchoredPosition = Vector2.zero;
        CenterAnchor(lockOverlay.rectTransform);
        
        // 4. Create or get Keybind Text
        if (keybindText == null)
        {
            GameObject keybindObj = new GameObject("Keybind");
            keybindObj.transform.SetParent(slotRect, false);
            keybindText = keybindObj.AddComponent<TextMeshProUGUI>();
            keybindText.fontStyle = FontStyles.Bold;
            keybindText.alignment = TextAlignmentOptions.Center;
            keybindText.color = Color.white;
            keybindText.outlineWidth = 0.2f;
            keybindText.outlineColor = Color.black;
        }
        // ALWAYS apply font size
        keybindText.fontSize = keybindFontSize;
        keybindText.rectTransform.anchorMin = new Vector2(0, 0);
        keybindText.rectTransform.anchorMax = new Vector2(1, 0);
        keybindText.rectTransform.pivot = new Vector2(0.5f, 0);
        keybindText.rectTransform.sizeDelta = new Vector2(0, 24);
        keybindText.rectTransform.anchoredPosition = new Vector2(0, 4);
        
        // Ensure correct rendering order
        border.transform.SetSiblingIndex(0);
        icon.transform.SetSiblingIndex(1);
        lockOverlay.transform.SetSiblingIndex(2);
        keybindText.transform.SetSiblingIndex(3);
    }
    
    Image CreateChildImage(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        return img;
    }
    
    void SetStretchFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
    
    void CenterAnchor(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    void UpdateState()
    {
        if (LevelGetter.Instance == null) return;
        
        bool unlocked = LevelGetter.Instance.CurrentLevel >= data.unlockLevel;
        
        if (lockOverlay != null)
        {
            lockOverlay.gameObject.SetActive(!unlocked);
        }
        
        if (icon != null)
        {
            icon.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
    
    // Public method to refresh state when level changes
    public void RefreshState()
    {
        if (data != null)
        {
            UpdateState();
        }
    }
}
