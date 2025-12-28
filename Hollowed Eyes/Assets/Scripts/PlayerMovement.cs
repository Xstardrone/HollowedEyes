using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.5f;

    private bool hasUsedGroundJump = false;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded = false;
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
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Reset ground jump when landing
        if (isGrounded && !wasGrounded)
        {
            hasUsedGroundJump = false;
        }
        
        wasGrounded = isGrounded;
        
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
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            if (isGrounded && !hasUsedGroundJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                hasUsedGroundJump = true;
            }
            else
            {
                if (PlayerMaskController.Instance != null && PlayerMaskController.Instance.CanUseBonusJump())
                {
                    // Consume a mask use
                    if (PlayerMaskController.Instance.UseBonusJump())
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    }
                }
            }
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

    public string GetFacing()
    {
        return facing;
    }
}