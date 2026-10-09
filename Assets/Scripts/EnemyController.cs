
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 30f;

    [Header("Attack")]
    public float contactDamage = 10f;
    public float damageInterval = 1f;

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
    private bool isDead;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyHealthDrop healthDrop;

    private Transform mecha;
    private Transform pilot;
    private Transform currentTarget;

    private bool isFlanker;
    private Vector3 flankOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        healthDrop = GetComponent<EnemyHealthDrop>();

        rb.isKinematic = true;
        rb.useGravity = false;

        agent.updateRotation = false;

        // Assign a behavior to this enemy.
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

        nextPathUpdateTime =
            Time.time + pathUpdateInterval;

        Vector3 destination = currentTarget.position;

        if (isFlanker)
        {
            float distanceToTarget = Vector3.Distance(
                transform.position,
                currentTarget.position
            );

            // Approach a position around the player.
            if (distanceToTarget > directAttackDistance)
            {
                destination += flankOffset;
            }
        }

        // Find a valid nearby point on the NavMesh.
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

        nextDamageTime =
            Time.time + damageInterval;
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

        if (healthDrop != null)
            healthDrop.TryDropHealth();

        Destroy(gameObject);
    }
}
