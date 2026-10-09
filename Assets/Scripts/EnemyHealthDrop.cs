
using UnityEngine;

public class EnemyHealthDrop : MonoBehaviour
{
    [Header("Health Drop")]
    public GameObject healthPickupPrefab;

    [Range(0f, 100f)]
    public float dropChance = 25f;

    [Header("Spawn Settings")]
    public float spawnHeight = 0.5f;

    private bool hasDropped = false;

    public void TryDropHealth()
    {
        if (hasDropped || healthPickupPrefab == null)
            return;

        hasDropped = true;

        if (Random.Range(0f, 100f) >= dropChance)
            return;

        Vector3 spawnPosition = transform.position;
        spawnPosition.y += spawnHeight;

        Instantiate(
            healthPickupPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}