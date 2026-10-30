
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 30f;

    [Header("XP Reward")]
    public int xpReward = 10;

    [Header("Attack")]
    public float contactDamage = 10f;
    public float damageInterval = 1f;
    public float attackRange = 3f;

    [Header("Animation")]
    public Animator enemyAnimator;
    public string idleAnimationName = "01_Scout_Bot_Idle";
    public string runAnimationName = "01_Scout_Bot_Run";
    public string attackAnimationName = "01_Scout_Bot_Attack";
    public float attackAnimationInterval = 1f;
    public float animationTransitionTime = 0.1f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Targeting")]
    public float pilotDetectionRange = 25f;
    public float pathUpdateInterval = 0.4f;

    [Header("Surrounding Behavior")]
    [Range(0f, 100f)]
    public float flankerChance = 30f;

    public float minimumFlankRadius = 3f;
    public float maximumFlankRadius = 7f;
    public float directAttackDistance = 2.5f;

    private float currentHealth;
    private float nextDamageTime;
    private float nextPathUpdateTime;
    private float nextAttackAnimationTime;
    private bool isDead;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyHealthDrop healthDrop;
    private ProgressionManager progressionManager;

    private Transform mecha;
    private Transform pilot;
    private Transform currentTarget;

    private bool isFlanker;
    private Vector3 flankOffset;

    private string currentAnimation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        healthDrop = GetComponent<EnemyHealthDrop>();

        rb.isKinematic = true;
        rb.useGravity = false;

        agent.updateRotation = false;

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<Animator>();

        isFlanker = Random.Range(0f, 100f) < flankerChance;

        if (isFlanker)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);

            float radius = Random.Range(
                minimumFlankRadius,
                maximumFlankRadius
            );

            flankOffset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
        }
    }

    void Start()
    {
        currentHealth = maxHealth;

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
        UpdatePath();
        RotateTowardTarget();
        UpdateAnimation();
    }

    void SelectTarget()
    {
        currentTarget = mecha;

        if (pilot != null && pilot.gameObject.activeInHierarchy)
        {
            float distanceToPilot = Vector3.Distance(
                transform.position,
                pilot.position
            );

            if (distanceToPilot <= pilotDetectionRange)
                currentTarget = pilot;
        }
    }

    void UpdatePath()
    {
        if (currentTarget == null || !agent.isOnNavMesh)
            return;

        if (Time.time < nextPathUpdateTime)
            return;

        nextPathUpdateTime = Time.time + pathUpdateInterval;

        Vector3 destination = currentTarget.position;

        if (isFlanker)
        {
            float distanceToTarget = Vector3.Distance(
                transform.position,
                currentTarget.position
            );

            if (distanceToTarget > directAttackDistance)
                destination += flankOffset;
        }

        if (NavMesh.SamplePosition(
            destination,
            out NavMeshHit hit,
            3f,
            NavMesh.AllAreas
        ))
        {
            agent.SetDestination(hit.position);
        }
    }

    void RotateTowardTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void UpdateAnimation()
    {
        if (enemyAnimator == null)
            return;

        if (currentTarget == null)
        {
            PlayAnimation(idleAnimationName);
            return;
        }

        Vector3 difference =
            currentTarget.position - transform.position;

        difference.y = 0f;

        float distance = difference.magnitude;

        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackAnimationTime)
            {
                enemyAnimator.CrossFadeInFixedTime(
                    attackAnimationName,
                    animationTransitionTime,
                    0,
                    0f
                );

                currentAnimation = attackAnimationName;

                nextAttackAnimationTime =
                    Time.time + Mathf.Max(
                        0.1f,
                        attackAnimationInterval
                    );
            }

            return;
        }

        if (agent.isOnNavMesh &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            PlayAnimation(runAnimationName);
        }
        else
        {
            PlayAnimation(idleAnimationName);
        }
    }

    void PlayAnimation(string animationName)
    {
        if (enemyAnimator == null ||
            currentAnimation == animationName)
            return;

        enemyAnimator.CrossFadeInFixedTime(
            animationName,
            animationTransitionTime
        );

        currentAnimation = animationName;
    }

    void OnCollisionEnter(Collision collision)
    {
        DealContactDamage(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        DealContactDamage(collision);
    }

    void DealContactDamage(Collision collision)
    {
        if (isDead || Time.time < nextDamageTime)
            return;

        PlayerHealth health =
            collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        health.TakeDamage(contactDamage);

        nextDamageTime = Time.time + damageInterval;
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

        // Award XP once per enemy death.
        if (progressionManager != null)
            progressionManager.RegisterKill(xpReward);

        // Preserve the existing repair pickup system.
        if (healthDrop != null)
            healthDrop.TryDropHealth();

        Destroy(gameObject);
    }
}
