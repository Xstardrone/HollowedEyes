using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectToggle : MonoBehaviour
{
    public enum ToggleKey
    {
        Q, E, R, F, Tab, Escape,
        Alpha1, Alpha2, Alpha3, Alpha4
    }

    [SerializeField] private ToggleKey toggleKey = ToggleKey.Q;
    [SerializeField] private GameObject target;

    void Update()
    {
        if (target == null || Keyboard.current == null) return;

        if (IsKeyPressed())
        {
            target.SetActive(!target.activeSelf);
        }
    }

    bool IsKeyPressed()
    {
        return toggleKey switch
        {
            ToggleKey.Q => Keyboard.current.qKey.wasPressedThisFrame,
            ToggleKey.E => Keyboard.current.eKey.wasPressedThisFrame,
            ToggleKey.R => Keyboard.current.rKey.wasPressedThisFrame,
            ToggleKey.F => Keyboard.current.fKey.wasPressedThisFrame,
            ToggleKey.Tab => Keyboard.current.tabKey.wasPressedThisFrame,
            ToggleKey.Escape => Keyboard.current.escapeKey.wasPressedThisFrame,
            ToggleKey.Alpha1 => Keyboard.current.digit1Key.wasPressedThisFrame,
            ToggleKey.Alpha2 => Keyboard.current.digit2Key.wasPressedThisFrame,
            ToggleKey.Alpha3 => Keyboard.current.digit3Key.wasPressedThisFrame,
            ToggleKey.Alpha4 => Keyboard.current.digit4Key.wasPressedThisFrame,
            _ => false
        };
    }
}
