
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;

    [Header("References")]
    public Transform player;
    public BoxCollider spawnBounds;

    [Header("Starting Difficulty")]
    public float startingSpawnInterval = 2f;
    public int startingMaxEnemies = 30;

    [Range(0f, 100f)]
    public float startingRangedChance = 10f;

    [Header("Spawn Position")]
    public float spawnDistance = 20f;
    public float minimumSpawnDistance = 12f;
    public float flyingSpawnHeight = 4f;
    public float navMeshSearchRadius = 8f;
    public int maximumSpawnAttempts = 30;

    [Header("Gradual Difficulty Scaling")]
    public float spawnIntervalReductionPerMinute = 0.08f;
    public int maxEnemyIncreasePerMinute = 4;
    public float rangedChanceIncreasePerMinute = 1f;

    [Header("Major Difficulty Spikes")]
    public float difficultySpikeIntervalMinutes = 10f;
    public float spikeSpawnIntervalMultiplier = 0.8f;
    public int spikeMaxEnemyIncrease = 20;
    public float spikeRangedChanceIncrease = 5f;

    [Header("Difficulty Limits")]
    public float minimumSpawnInterval = 0.25f;
    public int maximumEnemyCap = 250;

    [Range(0f, 100f)]
    public float maximumRangedChance = 60f;

    [Header("Testing")]
    public bool useDebugTimeMultiplier = false;
    public float debugTimeMultiplier = 60f;

    [Header("Current Difficulty (Read Only)")]
    [SerializeField] private float survivalTime;
    [SerializeField] private int currentMinute;
    [SerializeField] private int currentDifficultyTier;
    [SerializeField] private float currentSpawnInterval;
    [SerializeField] private int currentMaxEnemies;
    [SerializeField] private float currentRangedChance;

    private float spawnTimer;

    public float SurvivalTime => survivalTime;
    public int CurrentMinute => currentMinute;
    public int CurrentDifficultyTier => currentDifficultyTier;
    public float CurrentSpawnInterval => currentSpawnInterval;
    public int CurrentMaxEnemies => currentMaxEnemies;
    public float CurrentRangedChance => currentRangedChance;

    void OnEnable()
    {
        survivalTime = 0f;
        spawnTimer = 0f;
        currentMinute = 0;
        currentDifficultyTier = 0;

        UpdateDifficulty();

        Debug.Log("Survival timer started!");
    }

    void Update()
    {
        float timeMultiplier = useDebugTimeMultiplier
            ? Mathf.Max(1f, debugTimeMultiplier)
            : 1f;

        float elapsed = Time.deltaTime * timeMultiplier;

        survivalTime += elapsed;

        UpdateDifficulty();

        if (player == null)
            return;

        if (meleeEnemyPrefab == null &&
            rangedEnemyPrefab == null)
            return;

        spawnTimer += elapsed;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer -= currentSpawnInterval;

            if (transform.childCount < currentMaxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void UpdateDifficulty()
    {
        int newMinute = Mathf.FloorToInt(
            survivalTime / 60f
        );

        float spikeIntervalSeconds =
            Mathf.Max(1f, difficultySpikeIntervalMinutes) * 60f;

        int newTier = Mathf.FloorToInt(
            survivalTime / spikeIntervalSeconds
        );

        if (newTier > currentDifficultyTier)
        {
            Debug.Log(
                "DIFFICULTY SPIKE! Tier " +
                newTier +
                " reached at " +
                FormatTime(survivalTime)
            );
        }

        currentMinute = newMinute;
        currentDifficultyTier = newTier;

        float gradualSpawnInterval =
            startingSpawnInterval -
            currentMinute * spawnIntervalReductionPerMinute;

        currentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            gradualSpawnInterval *
            Mathf.Pow(
                spikeSpawnIntervalMultiplier,
                currentDifficultyTier
            )
        );

        currentMaxEnemies = Mathf.Clamp(
            startingMaxEnemies +
            currentMinute * maxEnemyIncreasePerMinute +
            currentDifficultyTier * spikeMaxEnemyIncrease,
            1,
            maximumEnemyCap
        );

        currentRangedChance = Mathf.Clamp(
            startingRangedChance +
            currentMinute * rangedChanceIncreasePerMinute +
            currentDifficultyTier * spikeRangedChanceIncrease,
            0f,
            maximumRangedChance
        );
    }

    void SpawnEnemy()
    {
        if (spawnBounds == null || !spawnBounds.enabled ||
            !spawnBounds.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "EnemySpawner: Assign an active EnemySpawnBounds BoxCollider."
            );
            return;
        }

        const int maxMeleeEnemies = 17;
        const int maxRangedEnemies = 8;
        int meleeCount = 0;
        int rangedCount = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform enemy = transform.GetChild(i);
            if (!enemy.gameObject.activeInHierarchy)
                continue;

            if (enemy.GetComponent<RangedEnemyController>() != null)
                rangedCount++;
            else if (enemy.GetComponent<EnemyController>() != null)
                meleeCount++;
        }

        bool canSpawnMelee = meleeEnemyPrefab != null && meleeCount < maxMeleeEnemies;
        bool canSpawnRanged = rangedEnemyPrefab != null && rangedCount < maxRangedEnemies;

        if (!canSpawnMelee && !canSpawnRanged)
            return;

        bool isRanged = canSpawnRanged &&
            (!canSpawnMelee || Random.Range(0f, 100f) < currentRangedChance);
        GameObject selectedPrefab = isRanged ? rangedEnemyPrefab : meleeEnemyPrefab;

        int attempts = Mathf.Max(1, maximumSpawnAttempts);

        for (int i = 0; i < attempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.right;

            float distance = Random.Range(
                Mathf.Min(minimumSpawnDistance, spawnDistance),
                Mathf.Max(minimumSpawnDistance, spawnDistance)
            );

            Vector3 candidate =
                player.position +
                new Vector3(
                    direction.x * distance,
                    0f,
                    direction.y * distance
                );

            if (!TryGetSpawnPosition(
                candidate,
                isRanged,
                out Vector3 spawnPosition
            ))
            {
                continue;
            }

            Instantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            return;
        }
    }

    bool TryGetSpawnPosition(
        Vector3 candidate,
        bool isRanged,
        out Vector3 spawnPosition
    )
    {
        spawnPosition = candidate;

        // Find walkable ground near the candidate.
        if (!NavMesh.SamplePosition(
            candidate,
            out NavMeshHit hit,
            navMeshSearchRadius,
            NavMesh.AllAreas
        ))
        {
            return false;
        }

        Vector3 groundPosition = hit.position;

        // Check the actual NavMesh point, not just
        // the original random candidate.
        if (!IsInsideBounds(groundPosition))
            return false;

        // Avoid spawning too close to the player.
        Vector2 playerXZ = new Vector2(
            player.position.x,
            player.position.z
        );

        Vector2 spawnXZ = new Vector2(
            groundPosition.x,
            groundPosition.z
        );

        if (Vector2.Distance(playerXZ, spawnXZ) <
            minimumSpawnDistance)
        {
            return false;
        }

        if (isRanged)
        {
            spawnPosition =
                groundPosition + Vector3.up * flyingSpawnHeight;
        }
        else
        {
            spawnPosition = groundPosition;
        }

        // Flying enemies must also fit inside the box.
        if (!IsInsideBounds(spawnPosition))
            return false;

        return true;
    }

    bool IsInsideBounds(Vector3 worldPosition)
    {
        Vector3 localPosition =
            spawnBounds.transform.InverseTransformPoint(
                worldPosition
            );

        Vector3 center = spawnBounds.center;
        Vector3 halfSize = spawnBounds.size * 0.5f;

        Vector3 offset = localPosition - center;

        return Mathf.Abs(offset.x) <= halfSize.x &&
               Mathf.Abs(offset.y) <= halfSize.y &&
               Mathf.Abs(offset.z) <= halfSize.z;
    }

    string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);

        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            remainingSeconds
        );
    }
}
