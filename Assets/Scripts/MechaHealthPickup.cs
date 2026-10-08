
using UnityEngine;

public class MechaHealthPickup : MonoBehaviour
{
    [Header("Repair")]
    public float healAmount = 25f;

    [Header("Appearance")]
    public float rotationSpeed = 90f;

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

        health.Heal(healAmount);

        Debug.Log("MECHA REPAIRED!");

        Destroy(gameObject);
    }
}
