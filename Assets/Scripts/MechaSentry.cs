
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MechaSentry : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Detection")]
    public float detectionRange = 18f;

    [Header("Combat")]
    public float rotationSpeed = 12f;
    public float fireRate = 0.25f;

    private float nextFireTime;
    private Transform currentTarget;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        currentTarget = null;
        nextFireTime = 0f;
    }

    void Update()
    {
        FindClosestEnemy();

        if (currentTarget != null)
        {
            ShootAtEnemy();
        }
    }

    void FixedUpdate()
    {
        if (currentTarget != null)
        {
            AimAtEnemy();
        }
    }

    void FindClosestEnemy()
    {
        EnemyController[] enemies =
            FindObjectsByType<EnemyController>(
                FindObjectsSortMode.None
            );

        float closestDistance = detectionRange;
        currentTarget = null;

        foreach (EnemyController enemy in enemies)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = enemy.transform;
            }
        }
    }

    void AimAtEnemy()
    {
        Vector3 direction =
            currentTarget.position - rb.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    void ShootAtEnemy()
    {
        if (Time.time < nextFireTime)
            return;

        if (bulletPrefab == null || firePoint == null)
            return;

        Vector3 direction =
            currentTarget.position - firePoint.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Only fire when facing the enemy.
        if (Vector3.Angle(transform.forward, direction) > 10f)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
        MechaAnimator animationController =
        GetComponent<MechaAnimator>();

        if (animationController != null)
            animationController.PlayShootAnimation();

        // Prevent bullets from hitting the mech.
        Collider bulletCollider =
            bullet.GetComponent<Collider>();

        if (bulletCollider != null)
        {
            Collider[] mechColliders =
                GetComponentsInChildren<Collider>();

            foreach (Collider mechCollider in mechColliders)
            {
                Physics.IgnoreCollision(
                    bulletCollider,
                    mechCollider
                );
            }
        }

        nextFireTime = Time.time + fireRate;
    }
}
