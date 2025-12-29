using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReset : MonoBehaviour
{
    [Header("Reset Position")]
    [SerializeField] private Vector3 resetPosition = new Vector3(0, 0, 0);
    [SerializeField] private float resetRotationZ = 0f;

    [Header("Reset Sound")]
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private float soundVolume = 1f;

    [Header("Boundary Detection")]
    [SerializeField] private GameObject backgroundObject;

    private Bounds backgroundBounds;
    private bool hasBounds = false;
    private float lastResetTime = -999f;
    private float resetCooldown = 0.5f;
    private AudioSource audioSource;

    void Start()
    {
        CalculateBackgroundBounds();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void CalculateBackgroundBounds()
    {
        if (backgroundObject == null)
        {
            hasBounds = false;
            return;
        }

        SpriteRenderer sr = backgroundObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            backgroundBounds = sr.bounds;
            hasBounds = true;
            return;
        }

        Collider2D col = backgroundObject.GetComponent<Collider2D>();
        if (col != null)
        {
            backgroundBounds = col.bounds;
            hasBounds = true;
            return;
        }

        hasBounds = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ResetPlayer();

        if (hasBounds && IsOutsideBounds() && Time.time - lastResetTime > resetCooldown)
            ResetPlayer();
    }

    bool IsOutsideBounds()
    {
        Vector3 p = transform.position;

        return p.x < backgroundBounds.min.x ||
               p.x > backgroundBounds.max.x ||
               p.y < backgroundBounds.min.y ||
               p.y > backgroundBounds.max.y;
    }

    public void ResetPlayer()
    {
        lastResetTime = Time.time;

        // Cut any active rope before resetting
        if (PlayerMaskController.Instance != null && PlayerMaskController.Instance.isRopeActive)
        {
            PlayerMaskController.Instance.CutRope();
        }

        transform.position = resetPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, resetRotationZ);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (PlayerMaskController.Instance != null)
            PlayerMaskController.Instance.ResetAllUses();

        if (resetSound != null)
            audioSource.PlayOneShot(resetSound, soundVolume);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
            ResetPlayer();
    }
}
