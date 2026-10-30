
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionHUD : MonoBehaviour
{
    [Header("Progression Reference")]
    public ProgressionManager progressionManager;

    [Header("UI References")]
    public Slider xpBar;
    public TMP_Text levelText;
    public TMP_Text xpText;

    void Start()
    {
        UpdateHUD();
    }

    void Update()
    {
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (progressionManager == null)
            return;

        int currentLevel = progressionManager.CurrentLevel;
        int currentXP = progressionManager.CurrentXP;
        int requiredXP = progressionManager.XPRequired;

        if (xpBar != null)
        {
            xpBar.minValue = 0f;
            xpBar.maxValue = Mathf.Max(1, requiredXP);
            xpBar.value = currentXP;
        }

        if (levelText != null)
            levelText.text = "LVL " + currentLevel;

        if (xpText != null)
            xpText.text = currentXP + " / " + requiredXP + " XP";
    }
}
