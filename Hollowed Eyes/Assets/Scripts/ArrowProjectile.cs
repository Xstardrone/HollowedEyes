using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private float lifetime;

    public void Init(float lifetime)
    {
        this.lifetime = lifetime;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
