
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 18f;
    public float damage = 10f;
    public float lifetime = 5f;

    private Rigidbody rb;
    private bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        // Ignore other enemy projectiles.
        if (other.GetComponentInParent<EnemyBullet>() != null)
            return;

        // Ignore melee enemies.
        if (other.GetComponentInParent<EnemyController>() != null)
            return;

        // Ignore ranged enemies.
        if (other.GetComponentInParent<RangedEnemyController>() != null)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHit = true;
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Ignore other triggers, such as detection zones.
        if (other.isTrigger)
            return;

        // Hit a wall, building, or other solid object.
        hasHit = true;
        Destroy(gameObject);
    }
}
