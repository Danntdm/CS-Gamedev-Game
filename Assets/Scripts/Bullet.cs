
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifetime = 3f;
    public float damage = 10f;

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
            rangedEnemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Ignore unrelated triggers.
        if (other.isTrigger)
            return;

        // Destroy the bullet when hitting a solid obstacle.
        hasHit = true;
        Destroy(gameObject);
    }
}
