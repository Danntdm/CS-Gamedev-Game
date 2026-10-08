
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stoppingDistance = 0.5f;

    [Header("Health")]
    public float maxHealth = 30f;

    [Header("Attack")]
    public float contactDamage = 10f;
    public float damageInterval = 1f;

    [Header("Targeting")]
    public float pilotDetectionRange = 25f;

    private float currentHealth;
    private float nextDamageTime;

    private Rigidbody rb;
    private Transform mecha;
    private Transform pilot;
    private Transform currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;

        // Find the mech using its Player tag.
        GameObject mechaObject =
            GameObject.FindGameObjectWithTag("Player");

        if (mechaObject != null)
            mecha = mechaObject.transform;

        // Find the pilot by its movement component.
        PilotMovement pilotMovement =
            FindFirstObjectByType<PilotMovement>(
                FindObjectsInactive.Include
            );

        if (pilotMovement != null)
            pilot = pilotMovement.transform;
    }

    void Update()
    {
        SelectTarget();
    }

    void SelectTarget()
    {
        // Default target is the mech.
        currentTarget = mecha;

        // Prioritize the pilot when outside the mech
        // and within detection range.
        if (pilot != null && pilot.gameObject.activeInHierarchy)
        {
            float distanceToPilot = Vector3.Distance(
                transform.position,
                pilot.position
            );

            if (distanceToPilot <= pilotDetectionRange)
            {
                currentTarget = pilot;
            }
        }
    }

    void FixedUpdate()
    {
        if (currentTarget == null)
            return;

        Vector3 direction =
            currentTarget.position - rb.position;

        direction.y = 0f;

        if (direction.magnitude > stoppingDistance)
        {
            Vector3 movement =
                direction.normalized *
                moveSpeed *
                Time.fixedDeltaTime;

            rb.MovePosition(rb.position + movement);
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
        if (Time.time < nextDamageTime)
            return;

        // Damage either the pilot or the mech
        // if they have a PlayerHealth component.
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
        currentHealth -= damage;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}
