
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

    [Header("Damage Feedback")]
    public DamageFlash damageFlash;

    [Header("Death Explosion")]
    public GameObject deathExplosionPrefab;
    public float explosionHeight = 1f;
    public float explosionLifetime = 5f;

    private float currentHealth;
    private bool isDead;
    private bool gameOver;

    private MechaSentry mechaSentry;

    void Awake()
    {
        // Only the mecha needs a sentry reference.
        if (!isPilot)
            mechaSentry = GetComponent<MechaSentry>();
    }

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
        if (isDead || damage <= 0f)
            return;

        // SENTRY INVULNERABILITY
        // Mecha cannot take damage when its
        // autonomous sentry component is active.
        // Pilot health is unaffected.
        if (!isPilot &&
            mechaSentry != null &&
            mechaSentry.isActiveAndEnabled)
        {
            return;
        }

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        UpdateHealthBar();

        if (damageFlash != null)
            damageFlash.Flash();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

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
        {
            healthBar.fillRect.gameObject.SetActive(
                currentHealth > 0f
            );
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        SpawnDeathExplosion();

        if (isPilot)
        {
            Debug.Log("PILOT KILLED!");

            PilotMovement movement =
                GetComponent<PilotMovement>();

            if (movement != null)
                movement.enabled = false;
        }
        else
        {
            Debug.Log("MECHA DESTROYED!");

            foreach (Renderer renderer in
                     GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            MechaMovement movement =
                GetComponent<MechaMovement>();

            if (movement != null)
                movement.enabled = false;

            MechaAim aim =
                GetComponent<MechaAim>();

            if (aim != null)
                aim.enabled = false;

            MechaWeapon weapon =
                GetComponent<MechaWeapon>();

            if (weapon != null)
                weapon.enabled = false;

            MechaSentry sentry =
                GetComponent<MechaSentry>();

            if (sentry != null)
                sentry.enabled = false;
        }

        TriggerGameOver();
    }

    void SpawnDeathExplosion()
    {
        if (deathExplosionPrefab == null)
            return;

        Vector3 explosionPosition = transform.position;
        explosionPosition.y += explosionHeight;

        GameObject explosion = Instantiate(
            deathExplosionPrefab,
            explosionPosition,
            Quaternion.identity
        );

        ParticleSystem[] particles =
            explosion.GetComponentsInChildren<ParticleSystem>(
                true
            );

        foreach (ParticleSystem particle in particles)
        {
            var main = particle.main;
            main.useUnscaledTime = true;
        }

        StartCoroutine(
            DestroyExplosionAfterDelay(explosion)
        );
    }

    System.Collections.IEnumerator DestroyExplosionAfterDelay(
        GameObject explosion)
    {
        yield return new WaitForSecondsRealtime(
            explosionLifetime
        );

        if (explosion != null)
            Destroy(explosion);
    }

    void TriggerGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning(
                "Game Over Panel is not assigned!"
            );

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
