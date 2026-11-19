
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 18f;
    public float damage = 10f;
    public float lifetime = 5f;

    [Header("Impact Effect")]
    public GameObject impactEffectPrefab;
    public float impactEffectLifetime = 1f;

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

            SpawnImpact(other);

            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Ignore other triggers, such as detection zones.
        if (other.isTrigger)
            return;

        // Hit a wall, building, or other solid object.
        hasHit = true;

        SpawnImpact(other);

        Destroy(gameObject);
    }

    void SpawnImpact(Collider hitCollider)
    {
        if (impactEffectPrefab == null)
            return;

        // Find the closest point on the object hit.
        Vector3 impactPosition =
            hitCollider.ClosestPoint(transform.position);

        // Spawn the effect independently of the bullet.
        GameObject impact = Instantiate(
            impactEffectPrefab,
            impactPosition,
            Quaternion.identity
        );

        // Clean up the effect after its lifetime.
        Destroy(
            impact,
            Mathf.Max(0.01f, impactEffectLifetime)
        );
    }
}
