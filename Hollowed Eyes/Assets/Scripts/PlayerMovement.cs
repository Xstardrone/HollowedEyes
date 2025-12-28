using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.5f;

    private int baseJumps = 1;
    private int hasJumps;
    private int bonusJumpsUsedThisAir = 0;  // Track bonus jumps used this airtime
    private bool refillJumps;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private string facing = "right";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -1.4f, 0);
            groundCheck = checkObj.transform;
        }
        
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
        }

        RefillJumpsToMax();
        refillJumps = true;
    }
    
    int GetMaxJumps()
    {
        int bonus = 0;
        if (PlayerMaskController.Instance != null && PlayerMaskController.Instance.CanUseBonusJump())
        {
            bonus = 1;
        }
        return baseJumps + bonus;
    }
    
    void RefillJumpsToMax()
    {
        hasJumps = GetMaxJumps();
        bonusJumpsUsedThisAir = 0;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Refill jumps when landing
        if (isGrounded && refillJumps)
        {
            RefillJumpsToMax();
        }
        
        // Get horizontal input
        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) {
                horizontalInput = -1f;
                facing = "left";
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) {
                horizontalInput = 1f;
                facing = "right";
            }
        }
        
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // Jump
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && hasJumps > 0)
        {
            // Check if this is a bonus jump (using more than base jumps)
            int jumpsUsed = (GetMaxJumps() - hasJumps);
            bool isBonusJump = jumpsUsed >= baseJumps;
            
            if (isBonusJump && PlayerMaskController.Instance != null)
            {
                // Consume a mask use for the bonus jump
                if (!PlayerMaskController.Instance.UseBonusJump())
                {
                    // No uses left, can't do bonus jump
                    return;
                }
                bonusJumpsUsedThisAir++;
            }
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            hasJumps--;
            refillJumps = false;
            Invoke("enableRefillJumps", 0.1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void enableRefillJumps()
    {
        refillJumps = true;
    }

    public string GetFacing()
    {
        return facing;
    }
}