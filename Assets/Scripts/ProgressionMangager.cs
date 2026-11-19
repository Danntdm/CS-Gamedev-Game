
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [Header("References")]
    public UpgradeManager upgradeManager;

    [Header("Level Progression")]
    public int startingXPRequirement = 100;
    public int additionalXPPerLevel = 50;

    [Header("Upgrade Effect")]
    public GameObject upgradeEffectPrefab;
    public Transform mechaTransform;
    public float effectHeight = 1f;
    public float effectLifetime = 2f;

    [Header("Mecha Upgrade Flash")]
    public EnemyHitFlash mechaUpgradeFlash;

    [Header("Current Progress (Read Only)")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpRequired = 100;
    [SerializeField] private int totalKills = 0;

    private int pendingUpgrades = 0;
    private bool waitingForUpgrade = false;

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPRequired => xpRequired;
    public int TotalKills => totalKills;

    void Start()
    {
        currentLevel = 1;
        currentXP = 0;
        totalKills = 0;
        pendingUpgrades = 0;
        waitingForUpgrade = false;

        xpRequired = Mathf.Max(1, startingXPRequirement);
    }

    public void RegisterKill(int xpReward)
    {
        totalKills++;

        currentXP += Mathf.Max(0, xpReward);

        Debug.Log(
            "Enemy killed! Total kills: " + totalKills +
            " | XP: " + currentXP + "/" + xpRequired
        );

        while (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            currentLevel++;

            pendingUpgrades++;

            xpRequired = Mathf.Max(
                1,
                startingXPRequirement +
                (currentLevel - 1) * additionalXPPerLevel
            );

            Debug.Log("LEVEL UP! Reached level " + currentLevel);
        }

        TryOpenUpgrade();
    }

    // Totems use the same upgrade queue as level-ups.
    public bool TryQueueUpgrade()
    {
        if (!isActiveAndEnabled ||
            upgradeManager == null ||
            !upgradeManager.isActiveAndEnabled ||
            upgradeManager.upgradePanel == null)
            return false;

        pendingUpgrades++;
        TryOpenUpgrade();
        return true;
    }

    void TryOpenUpgrade()
    {
        if (waitingForUpgrade ||
            pendingUpgrades <= 0 ||
            upgradeManager == null)
            return;

        waitingForUpgrade = true;
        upgradeManager.OpenUpgradeMenu();
    }

    // Called after the player chooses an upgrade.
    public void UpgradeSelected()
    {
        if (!waitingForUpgrade)
            return;

        // Play the surrounding upgrade particles.
        SpawnUpgradeEffect();

        // NEW: Flash the mecha blue.
        if (mechaUpgradeFlash != null)
            mechaUpgradeFlash.Flash();

        pendingUpgrades = Mathf.Max(
            0,
            pendingUpgrades - 1
        );

        waitingForUpgrade = false;

        TryOpenUpgrade();
    }

    void SpawnUpgradeEffect()
    {
        if (upgradeEffectPrefab == null ||
            mechaTransform == null)
            return;

        Vector3 effectPosition = mechaTransform.position;
        effectPosition.y += effectHeight;

        GameObject effect = Instantiate(
            upgradeEffectPrefab,
            effectPosition,
            Quaternion.identity
        );

        Destroy(effect, effectLifetime);
    }
}
