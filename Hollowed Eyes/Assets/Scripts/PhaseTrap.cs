using UnityEngine;

public class PhaseTrap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        
        // Check if player is in phase mode
        PlayerMaskController maskController = collision.gameObject.GetComponent<PlayerMaskController>();
        if (maskController != null && maskController.isPhasing)
        {
            // Player is phasing, they can pass through safely
            return;
        }
        
        // Player touched without phasing - trigger reset
        PlayerReset playerReset = collision.gameObject.GetComponent<PlayerReset>();
        if (playerReset != null)
        {
            playerReset.ResetPlayer();
        }
    }
}
