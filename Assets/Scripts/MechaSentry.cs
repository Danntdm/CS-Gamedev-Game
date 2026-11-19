
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
    public float firingAngle = 10f;

    private float nextFireTime;
    private Transform currentTarget;
    private Rigidbody rb;
    private MechaAnimator animationController;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animationController = GetComponent<MechaAnimator>();
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
            ShootAtEnemy();
    }

    void FixedUpdate()
    {
        if (currentTarget != null)
            AimAtEnemy();
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
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            Vector3 offset =
                enemy.transform.position - transform.position;

            offset.y = 0f;

            float distance = offset.magnitude;

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
            Quaternion.LookRotation(direction.normalized);

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

        // Check facing direction from the mecha's
        // center, matching its rotation calculation.
        Vector3 aimDirection =
            currentTarget.position - transform.position;

        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.Angle(
                transform.forward,
                aimDirection.normalized
            );

            if (angle > firingAngle)
                return;
        }

        // Calculate bullet direction from the gun
        // so projectiles originate at FirePoint.
        Vector3 bulletDirection =
            currentTarget.position - firePoint.position;

        bulletDirection.y = 0f;

        // Fallback for extremely close targets.
        if (bulletDirection.sqrMagnitude < 0.001f)
            bulletDirection = transform.forward;

        if (bulletDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion bulletRotation = Quaternion.LookRotation(
            bulletDirection.normalized,
            Vector3.up
        );

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            bulletRotation
        );

        if (animationController != null)
            animationController.PlayShootAnimation();

        // Prevent bullet collisions with the mecha.
        Collider[] bulletColliders =
            bullet.GetComponentsInChildren<Collider>();

        Collider[] mechColliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider bulletCollider in bulletColliders)
        {
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
