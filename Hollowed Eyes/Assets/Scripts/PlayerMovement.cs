using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.5f;

    [SerializeField] private GameObject spriteHolder;
    private Animator anim;

    private bool hasUsedGroundJump = false;
    private bool hasUsedAirJump = false; // Track if double jumped
    
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

        anim = spriteHolder.GetComponent<Animator>();
    }

    void Update()
    {

        Debug.Log(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("onGround", isGrounded);


        if (isGrounded)
        {
            anim.SetTrigger("hitGround");
            anim.ResetTrigger("Jump");
        }

        // Reset jumps
        if (isGrounded && !wasGrounded)
        {
            hasUsedGroundJump = false;
            hasUsedAirJump = false;
        }
        
        wasGrounded = isGrounded;
        
        // Get horizontal input
        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) {
                horizontalInput = -1f;
                facing = "left";
                spriteHolder.transform.localScale = new Vector3(-1, 1, 1);
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) {
                horizontalInput = 1f;
                facing = "right";
                spriteHolder.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        
        // Jump
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            if (isGrounded && !hasUsedGroundJump)
            {
                // single jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetTrigger("Jump");
                hasUsedGroundJump = true;
            }
            else if (!hasUsedAirJump)
            {
                // double jump
                if (PlayerMaskController.Instance != null && PlayerMaskController.Instance.CanUseBonusJump())
                {
                    if (PlayerMaskController.Instance.UseBonusJump())
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                        hasUsedAirJump = true;
                    }
                }
            }
        }

        anim.SetFloat("VertSpeed", rb.linearVelocityY);
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