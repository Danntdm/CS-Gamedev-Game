
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnDistance = 20f;
    public int maxEnemies = 30;

    private float spawnTimer;

    void Update()
    {
        if (player == null || enemyPrefab == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            if (transform.childCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        // Pick a random direction around the player.
        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        // Avoid a zero-length direction.
        if (randomDirection.sqrMagnitude < 0.01f)
            randomDirection = Vector2.right;

        Vector3 spawnPosition = player.position +
            new Vector3(
                randomDirection.x * spawnDistance,
                0f,
                randomDirection.y * spawnDistance
            );

        // Match the player's height for our flat arena.
        spawnPosition.y = player.position.y;

        // Spawn the enemy as a child of the spawner.
        Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );
    }
}
