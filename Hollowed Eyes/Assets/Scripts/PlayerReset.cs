using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReset : MonoBehaviour
{
    [Header("Reset Position")]
    [SerializeField] private Vector3 resetPosition = new Vector3(0, 0, 0);
    
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
        
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }
    
    void CalculateBackgroundBounds()
    {
        if (backgroundObject == null)
        {
            hasBounds = false;
            return;
        }
        
        // Try to get bounds from SpriteRenderer
        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            backgroundBounds = spriteRenderer.bounds;
            hasBounds = true;
            return;
        }
        
        // Try to get bounds from Collider2D
        Collider2D collider = backgroundObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            backgroundBounds = collider.bounds;
            hasBounds = true;
            return;
        }
        
        hasBounds = false;
    }
    
    void Update()
    {
        // Check for R key press
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetPlayer();
        }
        
        // Check if player is outside bounds (with cooldown to prevent infinite loop)
        if (hasBounds && IsOutsideBounds() && Time.time - lastResetTime > resetCooldown)
        {
            ResetPlayer();
        }
    }
    
    bool IsOutsideBounds()
    {
        Vector3 playerPos = transform.position;
        
        return playerPos.x < backgroundBounds.min.x ||
               playerPos.x > backgroundBounds.max.x ||
               playerPos.y < backgroundBounds.min.y ||
               playerPos.y > backgroundBounds.max.y;
    }
    
    void ResetPlayer()
    {
        lastResetTime = Time.time;
        transform.position = resetPosition;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Reset mask uses
        if (PlayerMaskController.Instance != null)
        {
            PlayerMaskController.Instance.ResetAllUses();
        }
        
        // Play reset sound
        if (resetSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(resetSound, soundVolume);
        }
        
        Debug.Log("Player reset to position: " + resetPosition);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Reset player when hitting a trap
        if (collision.gameObject.CompareTag("Trap"))
        {
            ResetPlayer();
        }
    }
}