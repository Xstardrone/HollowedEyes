[System.Serializable]
public class MaskData
{
    public string maskName;
    public int maskNumber;
    public int unlockLevel;  // Changed from levelUnlock to match JSON
    public string description;
    public string iconPath;  // Changed from iconImage to match JSON
    public string playerImagePath;  // Changed from playerImage to match JSON
    public string lockPath;  // Path to lock overlay image in Resources
    public string keybind;
    public string uiColor;
    public string unlockMessage;  // Message shown when mask is unlocked
    public float ypos;
}
