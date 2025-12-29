using UnityEngine;

public class WallSlide : MonoBehaviour
{
    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    
    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.52f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private bool isTouchingWall = false;
    private bool isGrounded = false;
    private PhysicsMaterial2D slipperyMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Create a slippery physics material to prevent wall sticking
        slipperyMaterial = new PhysicsMaterial2D("SlipperyWall");
        slipperyMaterial.friction = 0f;
        slipperyMaterial.bounciness = 0f;
        
        // Apply to player collider
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = slipperyMaterial;
        }
    }

    void Update()
    {
        CheckGround();
        CheckWall();
        
        // If touching wall and not grounded, ensure player slides down
        if (isTouchingWall && !isGrounded)
        {
            // Cancel upward velocity and any horizontal sticking
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
            
            // Add small downward force to ensure sliding
            rb.AddForce(Vector2.down * 2f, ForceMode2D.Force);
        }
    }
    
    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }
    
    void CheckWall()
    {
        // Cast rays to the left and right using groundLayer (since walls are on ground layer)
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        
        isTouchingWall = hitLeft.collider != null || hitRight.collider != null;
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize wall detection rays
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
        
        // Visualize ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}