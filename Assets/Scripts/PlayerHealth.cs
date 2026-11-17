
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Character Type")]
    public bool isPilot = false;

    [Header("UI")]
    public Slider healthBar;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private float currentHealth;
    private bool isDead;
    private bool gameOver;

    void Start()
    {
        Time.timeScale = 1f;
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (gameOver &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthBar();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }

    public void UpgradeArmor(float bonusHealth = 20f)
    {
        if (isDead)
            return;

        maxHealth += bonusHealth;
        currentHealth += bonusHealth;

        UpdateHealthBar();

        Debug.Log(
            "ARMOR UPGRADED! Maximum HP: " + maxHealth +
            " | Current HP: " + currentHealth
        );
    }

    void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        if (healthBar.fillRect != null)
            healthBar.fillRect.gameObject.SetActive(currentHealth > 0f);
    }

    void Die()
    {
        isDead = true;

        if (isPilot)
        {
            Debug.Log("PILOT KILLED!");

            PilotMovement movement = GetComponent<PilotMovement>();

            if (movement != null)
                movement.enabled = false;
        }
        else
        {
            Debug.Log("MECHA DESTROYED!");

            MechaMovement movement = GetComponent<MechaMovement>();

            if (movement != null)
                movement.enabled = false;

            MechaAim aim = GetComponent<MechaAim>();

            if (aim != null)
                aim.enabled = false;

            MechaWeapon weapon = GetComponent<MechaWeapon>();

            if (weapon != null)
                weapon.enabled = false;

            MechaSentry sentry = GetComponent<MechaSentry>();

            if (sentry != null)
                sentry.enabled = false;
        }

        // Either character dying ends the run.
        TriggerGameOver();
    }

    void TriggerGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning("Game Over Panel is not assigned!");

        Time.timeScale = 0f;

        Debug.Log("GAME OVER! Press R to restart.");
    }

    void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public bool IsDead()
    {
        return isDead;
    }
}
