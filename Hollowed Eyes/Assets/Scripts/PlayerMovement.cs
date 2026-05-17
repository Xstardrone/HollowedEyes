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
    [SerializeField] private float coyoteTime = 0.12f;

    private Animator anim;
    private bool hasUsedGroundJump = false;
    private bool hasUsedAirJump = false;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool wasGrounded = false;
    private float horizontalInput;
    private string facing = "right";
    private bool landCooldown = true;
    private bool animJumped = false;

    private float jumpResetCooldown = 0f;
    private const float JUMP_RESET_DELAY = 0.15f;
    private float coyoteCounter = 0f;

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

        playerCollider = GetComponent<Collider2D>();
        anim = spriteHolder.GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = EvaluateGroundedState();
        anim.SetBool("onGround", isGrounded);

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
            if (coyoteCounter < 0f)
            {
                coyoteCounter = 0f;
            }
        }

        if (isGrounded && landCooldown && animJumped)
        {
            anim.SetTrigger("hitGround");
            animJumped = false;
        }

        jumpResetCooldown -= Time.deltaTime;
        if (jumpResetCooldown < 0f)
        {
            jumpResetCooldown = 0f;
        }

        if (isGrounded && !wasGrounded && jumpResetCooldown <= 0f)
        {
            hasUsedGroundJump = false;
            hasUsedAirJump = false;
        }

        wasGrounded = isGrounded;

        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            bool leftPressed = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
            bool rightPressed = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
            bool canUseGroundJump = isGrounded || coyoteCounter > 0f;

            if (leftPressed)
            {
                horizontalInput = -1f;
                facing = "left";
                if (spriteHolder != null)
                {
                    spriteHolder.transform.localScale = new Vector3(-1, 1, 1);
                }
            }

            if (rightPressed)
            {
                horizontalInput = 1f;
                facing = "right";
                if (spriteHolder != null)
                {
                    spriteHolder.transform.localScale = new Vector3(1, 1, 1);
                }
            }

            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (canUseGroundJump && !hasUsedGroundJump)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("Jump");
                    landCooldown = false;
                    animJumped = true;
                    Invoke("EnableLandAnimation", 0.1f);
                    hasUsedGroundJump = true;
                    coyoteCounter = 0f;
                    jumpResetCooldown = JUMP_RESET_DELAY;
                }
                else if (!canUseGroundJump && !hasUsedAirJump)
                {
                    if (PlayerMaskController.Instance != null && PlayerMaskController.Instance.CanUseBonusJump())
                    {
                        if (PlayerMaskController.Instance.UseBonusJump())
                        {
                            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                            hasUsedAirJump = true;
                            jumpResetCooldown = JUMP_RESET_DELAY;
                        }
                    }
                }
            }
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetFloat("VertSpeed", rb.linearVelocityY);
    }

    bool EvaluateGroundedState()
    {
        if (playerCollider == null)
        {
            return false;
        }

        Bounds bounds = playerCollider.bounds;
        Vector2 footProbeCenter = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        Vector2 footProbeSize = new Vector2(Mathf.Max(0.05f, bounds.size.x * 0.8f), 0.1f);

        return Physics2D.OverlapBox(footProbeCenter, footProbeSize, 0f, groundLayer) != null;
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

    public void SetFacing(string newFacing)
    {
        facing = newFacing;
        if (spriteHolder != null)
        {
            if (newFacing == "left")
            {
                spriteHolder.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (newFacing == "right")
            {
                spriteHolder.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void ResetAirJump()
    {
        hasUsedAirJump = false;
    }

    public void EnableLandAnimation()
    {
        landCooldown = true;
    }
}