using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float spawnInterval = 5f;

    [Header("Movement")]
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float arrowLifetime = 5f;

    [Header("Rotation Offset")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 270);

    [Header("Spawn Offset")]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnArrow();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnArrow()
    {
        Vector3 spawnPos = transform.position + (Vector3)spawnOffset;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(rotationOffset + new Vector3(0, 0, angle));

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, rotation);

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (!rb)
        {
            rb = arrow.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }

        rb.linearVelocity = direction.normalized * arrowSpeed;

        ArrowProjectile proj = arrow.GetComponent<ArrowProjectile>();
        if (!proj) proj = arrow.AddComponent<ArrowProjectile>();
        proj.Init(arrowLifetime);
    }
}
