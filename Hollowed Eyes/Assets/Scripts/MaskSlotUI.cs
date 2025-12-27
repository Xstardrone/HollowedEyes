using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskSlotUI : MonoBehaviour
{
    [Header("Size Settings")]
    public float slotSize = 80f;
    public float iconSize = 64f;
    public float lockSize = 40f;
    public float keybindFontSize = 22f;
    public float usesFontSize = 22f;
    
    [Header("Font Settings")]
    public TMP_FontAsset customFont;
    
    [Header("References (Auto-created if null)")]
    public Image border;
    public Image icon;
    public Image lockOverlay;
    public TMP_Text keybindText;
    public TMP_Text usesText;

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
        RectTransform slotRect = GetComponent<RectTransform>();
        if (slotRect == null) slotRect = gameObject.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);
        
        // Load font if not assigned
        if (customFont == null)
        {
            customFont = Resources.Load<TMP_FontAsset>("Fonts/Robot-Bold SDF");
        }
        
        //Border (background)
        if (border == null)
        {
            border = CreateChildImage("Border", slotRect);
            border.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
        SetStretchFill(border.rectTransform);
        border.transform.SetAsFirstSibling();
        
        //Icon
        if (icon == null)
        {
            icon = CreateChildImage("Icon", slotRect);
            icon.color = Color.white;
        }
        icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        icon.rectTransform.anchoredPosition = Vector2.zero;
        CenterAnchor(icon.rectTransform);
        
        //Lock Overlay
        if (lockOverlay == null)
        {
            lockOverlay = CreateChildImage("Lock Overlay", slotRect);
            lockOverlay.color = Color.white;
        }
        lockOverlay.rectTransform.sizeDelta = new Vector2(lockSize, lockSize);
        lockOverlay.rectTransform.anchoredPosition = Vector2.zero;
        CenterAnchor(lockOverlay.rectTransform);
        
        //Uses Text (bottom LEFT, black)
        if (usesText == null)
        {
            GameObject usesObj = new GameObject("Uses");
            usesObj.transform.SetParent(slotRect, false);
            usesText = usesObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Apply font and settings
        if (customFont != null)
        {
            usesText.font = customFont;
        }
        usesText.fontSize = usesFontSize;
        usesText.fontStyle = FontStyles.Bold;
        usesText.alignment = TextAlignmentOptions.BottomLeft;
        usesText.color = Color.black;
        usesText.enableAutoSizing = false;
        usesText.rectTransform.anchorMin = new Vector2(0, 0);
        usesText.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
        usesText.rectTransform.pivot = new Vector2(0, 0);
        usesText.rectTransform.sizeDelta = Vector2.zero;
        usesText.rectTransform.anchoredPosition = new Vector2(4, 2);
        
        //Keybind Text (bottom RIGHT, white)
        if (keybindText == null)
        {
            GameObject keybindObj = new GameObject("Keybind");
            keybindObj.transform.SetParent(slotRect, false);
            keybindText = keybindObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Apply font and settings
        if (customFont != null)
        {
            keybindText.font = customFont;
        }
        keybindText.fontSize = keybindFontSize;
        keybindText.fontStyle = FontStyles.Bold;
        keybindText.alignment = TextAlignmentOptions.BottomRight;
        keybindText.color = Color.white;
        keybindText.outlineWidth = 0.2f;
        keybindText.outlineColor = Color.black;
        keybindText.enableAutoSizing = false;
        keybindText.rectTransform.anchorMin = new Vector2(0.5f, 0);
        keybindText.rectTransform.anchorMax = new Vector2(1, 0.3f);
        keybindText.rectTransform.pivot = new Vector2(1, 0);
        keybindText.rectTransform.sizeDelta = Vector2.zero;
        keybindText.rectTransform.anchoredPosition = new Vector2(-4, 2);
        
        // Correct rendering order
        border.transform.SetSiblingIndex(0);
        icon.transform.SetSiblingIndex(1);
        lockOverlay.transform.SetSiblingIndex(2);
        usesText.transform.SetSiblingIndex(3);
        keybindText.transform.SetSiblingIndex(4);
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
        
        // Update uses count
        if (usesText != null && PlayerMaskController.Instance != null)
        {
            int uses = PlayerMaskController.Instance.GetUsesForMask(data.maskNumber);
            usesText.text = uses.ToString();
        }
    }
    
    public void RefreshState()
    {
        if (data != null)
        {
            UpdateState();
        }
    }
}