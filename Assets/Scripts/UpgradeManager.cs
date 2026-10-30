
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject upgradePanel;

    [Header("Mecha References")]
    public MechaWeapon mechaWeapon;
    public PlayerHealth mechaHealth;

    [Header("Progression")]
    public ProgressionManager progressionManager;

    [Header("Armor Upgrade")]
    public float armorHealthBonus = 20f;

    private bool menuOpen = false;

    void Start()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public void OpenUpgradeMenu()
    {
        if (menuOpen || upgradePanel == null)
            return;

        menuOpen = true;
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("LEVEL UP! Choose an upgrade.");
    }

    public void SelectUpgrade(int upgradeIndex)
    {
        if (!menuOpen)
            return;

        switch (upgradeIndex)
        {
            case 0:
                if (mechaWeapon != null)
                    mechaWeapon.UpgradeDamage();
                break;

            case 1:
                if (mechaHealth != null)
                    mechaHealth.UpgradeArmor(armorHealthBonus);
                break;

            case 2:
                if (mechaWeapon != null)
                    mechaWeapon.UpgradeFireRate();
                break;

            default:
                return;
        }

        menuOpen = false;
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;

        // Tell progression that this upgrade has been chosen.
        if (progressionManager != null)
            progressionManager.UpgradeSelected();
    }

    void OnDisable()
    {
        if (menuOpen)
        {
            menuOpen = false;
            Time.timeScale = 1f;
        }
    }
}
