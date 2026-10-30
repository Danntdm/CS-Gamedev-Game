
using UnityEngine;
using TMPro;

public class SurvivalHUD : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public TMP_Text timerText;

    void Start()
    {
        if (timerText == null)
            timerText = GetComponent<TMP_Text>();

        UpdateTimer();
    }

    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (timerText == null)
            return;

        float elapsedTime = 0f;

        if (enemySpawner != null)
            elapsedTime = enemySpawner.SurvivalTime;

        int totalSeconds = Mathf.FloorToInt(elapsedTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }
}
