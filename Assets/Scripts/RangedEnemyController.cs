
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RangedEnemyController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 30f;

    [Header("XP Reward")]
    public int xpReward = 20;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float preferredDistance = 12f;
    public float retreatDistance = 7f;

    [Header("Flying")]
    public float hoverHeight = 4f;
    public float verticalSpeed = 4f;
    public float hoverAmount = 0.3f;
    public float hoverFrequency = 2f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayers;
    public float detectionDistance = 8f;
    public float avoidanceRadius = 1.5f;
    public float avoidanceStrength = 2f;

    [Header("Targeting")]
    public float pilotDetectionRange = 25f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float shootingRange = 20f;

    private Rigidbody rb;
    private Transform mecha;
    private Transform pilot;
    private Transform currentTarget;

    private float currentHealth;
    private float hoverOffset;
    private float nextFireTime;
    private bool isDead;

    private EnemyHealthDrop healthDrop;
    private ProgressionManager progressionManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        currentHealth = maxHealth;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);
        healthDrop = GetComponent<EnemyHealthDrop>();
    }

    void Start()
    {
        progressionManager =
            FindFirstObjectByType<ProgressionManager>();

        GameObject mechaObject =
            GameObject.FindGameObjectWithTag("Player");

        if (mechaObject != null)
            mecha = mechaObject.transform;

        PilotMovement pilotMovement =
            FindFirstObjectByType<PilotMovement>(
                FindObjectsInactive.Include
            );

        if (pilotMovement != null)
            pilot = pilotMovement.transform;
    }

    void Update()
    {
        if (isDead)
            return;

        SelectTarget();
        TryShoot();
    }

    void FixedUpdate()
    {
        if (isDead || currentTarget == null)
            return;

        MoveTowardTarget();
        FaceTarget();
    }

    void SelectTarget()
    {
        currentTarget = mecha;

        if (pilot != null && pilot.gameObject.activeInHierarchy)
        {
            float distanceToPilot =
                Vector3.Distance(transform.position, pilot.position);

            if (distanceToPilot <= pilotDetectionRange)
                currentTarget = pilot;
        }
    }

    bool IsPathBlocked(Vector3 direction, float distance)
    {
        if (direction.sqrMagnitude < 0.001f)
            return false;

        return Physics.SphereCast(
            rb.position,
            avoidanceRadius,
            direction.normalized,
            out _,
            distance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    Vector3 GetAvoidanceDirection(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude < 0.001f)
            return Vector3.zero;

        desiredDirection.Normalize();

        if (!IsPathBlocked(desiredDirection, detectionDistance))
            return desiredDirection;

        float[] angles =
        {
            30f, -30f, 60f, -60f, 90f,
            -90f, 135f, -135f, 180f
        };

        foreach (float angle in angles)
        {
            Vector3 alternative =
                Quaternion.Euler(0f, angle, 0f) * desiredDirection;

            if (!IsPathBlocked(alternative, detectionDistance))
                return alternative;
        }

        return Vector3.zero;
    }

    void MoveTowardTarget()
    {
        Vector3 currentPosition = rb.position;
        Vector3 targetPosition = currentTarget.position;

        Vector3 horizontalDirection =
            targetPosition - currentPosition;

        horizontalDirection.y = 0f;

        float horizontalDistance = horizontalDirection.magnitude;
        Vector3 movement = Vector3.zero;

        if (horizontalDistance > 0.01f)
        {
            Vector3 direction = horizontalDirection.normalized;

            if (horizontalDistance > preferredDistance)
                movement = direction;
            else if (horizontalDistance < retreatDistance)
                movement = -direction;
        }

        if (movement.sqrMagnitude > 0.001f)
        {
            movement = GetAvoidanceDirection(movement);
            movement *= moveSpeed;
        }

        float targetHeight =
            targetPosition.y + hoverHeight +
            Mathf.Sin(
                Time.time * hoverFrequency + hoverOffset
            ) * hoverAmount;

        float newY = Mathf.MoveTowards(
            currentPosition.y,
            targetHeight,
            verticalSpeed * Time.fixedDeltaTime
        );

        Vector3 newPosition =
            currentPosition + movement * Time.fixedDeltaTime;

        newPosition.y = newY;

        Vector3 displacement = newPosition - currentPosition;

        if (displacement.sqrMagnitude > 0.000001f &&
            !IsPathBlocked(
                displacement.normalized,
                displacement.magnitude + 0.1f
            ))
        {
            rb.MovePosition(newPosition);
        }
    }

    void FaceTarget()
    {
        Vector3 direction =
            currentTarget.position - rb.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }

    void TryShoot()
    {
        if (currentTarget == null ||
            bulletPrefab == null ||
            firePoint == null ||
            Time.time < nextFireTime)
            return;

        Vector3 aimPoint = currentTarget.position;

        PlayerHealth targetHealth =
            currentTarget.GetComponentInChildren<PlayerHealth>();

        if (targetHealth != null)
        {
            Collider targetCollider =
                targetHealth.GetComponentInChildren<Collider>();

            if (targetCollider != null)
                aimPoint = targetCollider.bounds.center;
        }

        Vector3 direction = aimPoint - firePoint.position;

        if (direction.sqrMagnitude < 0.001f ||
            direction.magnitude > shootingRange)
            return;

        Quaternion bulletRotation =
            Quaternion.LookRotation(direction.normalized);

        Instantiate(
            bulletPrefab,
            firePoint.position,
            bulletRotation
        );

        nextFireTime = Time.time + Mathf.Max(0.1f, fireRate);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (progressionManager != null)
            progressionManager.RegisterKill(xpReward);

        if (healthDrop != null)
            healthDrop.TryDropHealth();

        Destroy(gameObject);
    }
}
