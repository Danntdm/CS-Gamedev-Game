
using UnityEngine;

public class MechaHealthPickup : MonoBehaviour
{
    [Header("Repair")]
    public float healAmount = 25f;

    [Header("Appearance")]
    public float rotationSpeed = 90f;

    [Header("Healing Effect")]
    public GameObject healingEffectPrefab;
    public float effectHeight = 1f;
    public float effectLifetime = 2f;

    void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );
    }

    void OnTriggerEnter(Collider other)
    {
        MechaMovement movement =
            other.GetComponentInParent<MechaMovement>();

        if (movement == null)
            return;

        PlayerHealth health =
            movement.GetComponent<PlayerHealth>();

        if (health == null || health.IsDead())
            return;

        // Restore the mecha's health.
        health.Heal(healAmount);

        // Spawn the healing effect around the mecha.
        SpawnHealingEffect(movement.transform);

        Debug.Log("MECHA REPAIRED!");

        Destroy(gameObject);
    }

    void SpawnHealingEffect(Transform mecha)
    {
        if (healingEffectPrefab == null)
            return;

        Vector3 effectPosition = mecha.position;
        effectPosition.y += effectHeight;

        GameObject effect = Instantiate(
            healingEffectPrefab,
            effectPosition,
            Quaternion.identity
        );

        Destroy(effect, effectLifetime);
    }
}
