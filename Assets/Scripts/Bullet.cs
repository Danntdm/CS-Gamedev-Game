
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet")]
    public float speed = 35f;
    public float lifetime = 3f;
    public float damage = 10f;

    [Header("Impact Effect")]
    public GameObject impactEffectPrefab;
    public float impactEffectLifetime = 1f;

    private Rigidbody rb;
    private bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    // Called by the weapon when the bullet is created.
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        // Ignore the mech and pilot.
        if (other.GetComponentInParent<PlayerHealth>() != null ||
            other.CompareTag("Player"))
            return;

        // Damage ground enemies.
        EnemyController meleeEnemy =
            other.GetComponentInParent<EnemyController>();

        if (meleeEnemy != null)
        {
            hasHit = true;

            SpawnImpact(other);

            meleeEnemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Damage flying ranged enemies.
        RangedEnemyController rangedEnemy =
            other.GetComponentInParent<RangedEnemyController>();

        if (rangedEnemy != null)
        {
            hasHit = true;

            SpawnImpact(other);

            rangedEnemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Ignore unrelated triggers.
        if (other.isTrigger)
            return;

        // Hit a solid obstacle.
        hasHit = true;

        SpawnImpact(other);

        Destroy(gameObject);
    }

    void SpawnImpact(Collider hitCollider)
    {
        if (impactEffectPrefab == null)
            return;

        // Find the point on the collider closest to the bullet.
        Vector3 impactPosition =
            hitCollider.ClosestPoint(transform.position);

        // Create the effect independently of the bullet.
        GameObject impact = Instantiate(
            impactEffectPrefab,
            impactPosition,
            Quaternion.identity
        );

        // Remove the effect after its lifetime.
        Destroy(
            impact,
            Mathf.Max(0.01f, impactEffectLifetime)
        );
    }
}
