using UnityEngine;

public class NodeSetup : MonoBehaviour
{
    private SpringJoint2D springJoint;
    
    void Start()
    {
        // Get the SpringJoint2D component on this node
        springJoint = GetComponent<SpringJoint2D>();
        
        if (springJoint == null)
        {
            Debug.LogError("No SpringJoint2D found on " + gameObject.name);
            return;
        }
        
        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found!");
            return;
        }
        
        // Get the player's Rigidbody2D
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        
        if (playerRb == null)
        {
            return;
        }
        
        // Connect the spring joint to the player
        springJoint.connectedBody = playerRb;
        
    }
}