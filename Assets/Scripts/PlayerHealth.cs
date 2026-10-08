
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Character Type")]
    public bool isPilot = false;

    [Header("UI")]
    public Slider healthBar;

    private float currentHealth;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
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
    }

    public bool IsDead()
    {
        return isDead;
    }
}
