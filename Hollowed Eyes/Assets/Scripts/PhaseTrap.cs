using UnityEngine;

public class PhaseTrap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other.gameObject);
    }
    
    private void HandlePlayerContact(GameObject contactObject)
    {
        if (!contactObject.CompareTag("Player"))
        {
            return;
        }
        
        
        // Check if player is in phase mode
        PlayerMaskController maskController = contactObject.GetComponent<PlayerMaskController>();
        if (maskController == null)
        {
            Debug.LogWarning("PlayerMaskController not found on player!");
            return;
        }
        
        
        if (maskController.isPhasing)
        {
            // Player is phasing, they can pass through safely
            return;
        }
        
        
        // Player touched without phasing - trigger reset
        PlayerReset playerReset = contactObject.GetComponent<PlayerReset>();
        if (playerReset != null)
        {
            playerReset.ResetPlayer();
        }
        else
        {
        }
    }
}
