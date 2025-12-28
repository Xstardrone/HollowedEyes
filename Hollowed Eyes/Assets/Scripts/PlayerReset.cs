using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReset : MonoBehaviour
{
    [Header("Reset Position")]
    [SerializeField] private Vector3 resetPosition = new Vector3(0, 0, 0);
    
    void Update()
    {
        // Check for R key press
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetPlayer();
        }
    }
    
    void ResetPlayer()
    {
        transform.position = resetPosition;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Reset mask uses
        if (PlayerMaskController.Instance != null)
        {
            PlayerMaskController.Instance.ResetAllUses();
        }
        
        Debug.Log("Player reset to position: " + resetPosition);
    }
}