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
    [SerializeField] private float groundContactBelowCenterThreshold = 0.12f;

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
    private readonly ContactPoint2D[] contactPoints = new ContactPoint2D[32];

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
                if (isGrounded && !hasUsedGroundJump)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("Jump");
                    landCooldown = false;
                    animJumped = true;
                    Invoke("EnableLandAnimation", 0.1f);
                    hasUsedGroundJump = true;
                    jumpResetCooldown = JUMP_RESET_DELAY;
                }
                else if (!isGrounded && !hasUsedAirJump)
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

        int contactCount = playerCollider.GetContacts(contactPoints);
        float playerCenterY = playerCollider.bounds.center.y;

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contactPoints[i];
            Collider2D other = contact.collider;
            if (other == null)
                continue;

            int otherLayerMask = 1 << other.gameObject.layer;
            if ((groundLayer.value & otherLayerMask) == 0)
                continue;

            bool isBelowCenter = contact.point.y <= playerCenterY - groundContactBelowCenterThreshold;
            bool isUpwardEnough = contact.normal.y > 0.55f;

            if (isUpwardEnough && isBelowCenter)
            {
                return true;
            }
        }

        return false;
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