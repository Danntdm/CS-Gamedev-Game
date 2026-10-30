
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;

    [Header("References")]
    public Transform player;

    [Header("Starting Difficulty")]
    public float startingSpawnInterval = 2f;
    public int startingMaxEnemies = 30;

    [Range(0f, 100f)]
    public float startingRangedChance = 10f;

    [Header("Spawn Position")]
    public float spawnDistance = 20f;
    public float flyingSpawnHeight = 4f;
    public float navMeshSearchRadius = 8f;

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

        // Spawn speed scales with the survival timer.
        spawnTimer += elapsed;

        // Limit spawning to one enemy per frame, even
        // when debug time is heavily accelerated.
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

        // Gradually reduce the time between spawns.
        float gradualSpawnInterval =
            startingSpawnInterval -
            currentMinute * spawnIntervalReductionPerMinute;

        // Apply an additional reduction every spike.
        currentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            gradualSpawnInterval *
            Mathf.Pow(
                spikeSpawnIntervalMultiplier,
                currentDifficultyTier
            )
        );

        // Increase the number of active enemies allowed.
        currentMaxEnemies = Mathf.Clamp(
            startingMaxEnemies +
            currentMinute * maxEnemyIncreasePerMinute +
            currentDifficultyTier * spikeMaxEnemyIncrease,
            1,
            maximumEnemyCap
        );

        // Increase the chance of spawning ranged enemies.
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
        bool spawnRanged =
            Random.Range(0f, 100f) < currentRangedChance;

        GameObject selectedPrefab =
            spawnRanged ? rangedEnemyPrefab : meleeEnemyPrefab;

        // Fall back if one enemy type isn't assigned.
        if (selectedPrefab == null)
        {
            selectedPrefab =
                spawnRanged ? meleeEnemyPrefab : rangedEnemyPrefab;
        }

        if (selectedPrefab == null)
            return;

        bool isRanged =
            selectedPrefab == rangedEnemyPrefab;

        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        if (randomDirection.sqrMagnitude < 0.01f)
            randomDirection = Vector2.right;

        Vector3 spawnPosition =
            player.position +
            new Vector3(
                randomDirection.x * spawnDistance,
                0f,
                randomDirection.y * spawnDistance
            );

        if (isRanged)
        {
            spawnPosition.y =
                player.position.y + flyingSpawnHeight;
        }
        else
        {
            if (NavMesh.SamplePosition(
                spawnPosition,
                out NavMeshHit hit,
                navMeshSearchRadius,
                NavMesh.AllAreas
            ))
            {
                spawnPosition = hit.position;
            }
            else
            {
                return;
            }
        }

        Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );
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
