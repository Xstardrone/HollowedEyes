using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private float exitRotationZ = 180f;
    
    [Header("Exit Direction")]
    [Tooltip("Direction player faces after teleport. Leave empty to keep current direction.")]
    [SerializeField] private string exitDirection = ""; // "left", "right", or empty for no change
    
    [Tooltip("Flip horizontal velocity on exit (multiply X velocity by -1)")]
    [SerializeField] private bool flipHorizontalVelocity = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        Vector2 velocity = Vector2.zero;
        float angularVelocity = 0f;

        if (rb != null)
        {
            velocity = rb.linearVelocity;
            angularVelocity = rb.angularVelocity;
        }

        other.transform.position = teleportTarget.position;
        other.transform.rotation = Quaternion.Euler(0f, 0f, exitRotationZ);

        if (rb != null)
        {
            Vector2 rotatedVelocity = Quaternion.Euler(0f, 0f, exitRotationZ) * velocity;
            
            // Optionally flip horizontal velocity
            if (flipHorizontalVelocity)
            {
                rotatedVelocity.x = -rotatedVelocity.x;
            }
            
            rb.linearVelocity = rotatedVelocity;
            rb.angularVelocity = angularVelocity;
        }
        
        // Set player facing direction if specified
        if (!string.IsNullOrEmpty(exitDirection))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetFacing(exitDirection.ToLower());
            }
        }
    }
}
