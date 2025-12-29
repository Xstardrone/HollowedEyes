using UnityEngine;

public class PhaseTrap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"PhaseTrap trigger entered by: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        HandlePlayerContact(other.gameObject);
    }
    
    private void HandlePlayerContact(GameObject contactObject)
    {
        if (!contactObject.CompareTag("Player"))
        {
            Debug.Log($"Not player, ignoring. Tag was: {contactObject.tag}");
            return;
        }
        
        Debug.Log("PhaseTrap touched by player!");
        
        // Check if player is in phase mode
        PlayerMaskController maskController = contactObject.GetComponent<PlayerMaskController>();
        if (maskController == null)
        {
            Debug.LogWarning("PlayerMaskController not found on player!");
            return;
        }
        
        Debug.Log($"Player isPhasing: {maskController.isPhasing}, Active Mask: {maskController.activeMask}");
        
        if (maskController.isPhasing)
        {
            // Player is phasing, they can pass through safely
            Debug.Log("Player is phasing - passing through safely");
            return;
        }
        
        Debug.Log("Player not phasing - triggering reset");
        
        // Player touched without phasing - trigger reset
        PlayerReset playerReset = contactObject.GetComponent<PlayerReset>();
        if (playerReset != null)
        {
            playerReset.ResetPlayer();
        }
        else
        {
            Debug.LogWarning("PlayerReset component not found!");
        }
    }
}
