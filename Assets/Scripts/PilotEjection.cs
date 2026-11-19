
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotEjection : MonoBehaviour
{
    [Header("References")]
    public GameObject mecha;
    public GameObject pilot;
    public CameraFollow cameraFollow;

    [Header("Ejection Settings")]
    public float ejectDistance = 3f;
    public float enterDistance = 3f;
    public float interactionCooldown = 0.3f;

    [Header("Pilot Regeneration")]
    public float pilotRegenRate = 5f;

    [Header("Ejection Effect")]
    public GameObject ejectionEffectPrefab;
    public float ejectionEffectHeight = 1f;
    public float ejectionEffectLifetime = 2f;

    [Header("Reentry Effect")]
    public GameObject reentryEffectPrefab;
    public float reentryEffectHeight = 1f;
    public float reentryEffectLifetime = 2f;

    private bool insideMecha = true;
    private float nextInteractionTime;

    private MechaMovement mechaMovement;
    private MechaAim mechaAim;
    private MechaWeapon mechaWeapon;
    private MechaSentry mechaSentry;
    private PilotMovement pilotMovement;

    private PlayerHealth mechaHealth;
    private PlayerHealth pilotHealth;

    private Rigidbody mechaRb;

    void Awake()
    {
        mechaMovement = mecha.GetComponent<MechaMovement>();
        mechaAim = mecha.GetComponent<MechaAim>();
        mechaWeapon = mecha.GetComponent<MechaWeapon>();
        mechaSentry = mecha.GetComponent<MechaSentry>();
        mechaRb = mecha.GetComponent<Rigidbody>();

        pilotMovement = pilot.GetComponent<PilotMovement>();

        mechaHealth = mecha.GetComponent<PlayerHealth>();
        pilotHealth = pilot.GetComponent<PlayerHealth>();
    }

    void Start()
    {
        insideMecha = true;

        pilot.SetActive(false);
        mechaSentry.enabled = false;

        mechaMovement.enabled = true;
        mechaAim.enabled = true;
        mechaWeapon.enabled = true;

        cameraFollow.target = mecha.transform;
    }

    void Update()
    {
        // Regenerate pilot health while inside the mech.
        if (insideMecha &&
            pilotHealth != null &&
            !pilotHealth.IsDead())
        {
            pilotHealth.Heal(
                pilotRegenRate * Time.deltaTime
            );
        }

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        if (Time.time < nextInteractionTime)
            return;

        if (insideMecha)
        {
            if (mechaHealth != null && mechaHealth.IsDead())
                return;

            Eject();
        }
        else
        {
            if (pilotHealth != null && pilotHealth.IsDead())
                return;

            TryEnterMecha();
        }
    }

    void Eject()
    {
        insideMecha = false;

        // Disable manual mech control.
        mechaMovement.enabled = false;
        mechaAim.enabled = false;
        mechaWeapon.enabled = false;

        // Stop mech movement.
        mechaRb.linearVelocity = Vector3.zero;
        mechaRb.angularVelocity = Vector3.zero;

        // Activate Sentry Mode.
        if (mechaHealth == null || !mechaHealth.IsDead())
            mechaSentry.enabled = true;

        // Spawn pilot beside the mech.
        Vector3 spawnPosition =
            mecha.transform.position +
            mecha.transform.right * ejectDistance;

        spawnPosition.y =
            mecha.transform.position.y - 0.5f;

        pilot.transform.position = spawnPosition;
        pilot.SetActive(true);

        pilotMovement.enabled = true;

        // NEW: Play ejection effect.
        SpawnEffect(
            ejectionEffectPrefab,
            ejectionEffectHeight,
            ejectionEffectLifetime
        );

        // Follow pilot.
        cameraFollow.target = pilot.transform;

        nextInteractionTime =
            Time.time + interactionCooldown;

        Debug.Log("PILOT EJECTED - SENTRY MODE ACTIVE");
    }

    void TryEnterMecha()
    {
        if (mechaHealth != null && mechaHealth.IsDead())
        {
            Debug.Log("Cannot enter destroyed mech!");
            return;
        }

        float distance = Vector3.Distance(
            pilot.transform.position,
            mecha.transform.position
        );

        if (distance > enterDistance)
        {
            Debug.Log("Too far away to enter mech!");
            return;
        }

        insideMecha = true;

        // NEW: Play reentry effect before hiding pilot.
        SpawnEffect(
            reentryEffectPrefab,
            reentryEffectHeight,
            reentryEffectLifetime
        );

        // Disable Sentry Mode.
        mechaSentry.enabled = false;

        // Hide pilot.
        pilotMovement.enabled = false;
        pilot.SetActive(false);

        // Restore manual mech controls.
        mechaMovement.enabled = true;
        mechaAim.enabled = true;
        mechaWeapon.enabled = true;

        // Follow mech.
        cameraFollow.target = mecha.transform;

        nextInteractionTime =
            Time.time + interactionCooldown;

        Debug.Log("PILOT ENTERED MECHA - MANUAL CONTROL");
    }

    void SpawnEffect(
        GameObject effectPrefab,
        float height,
        float lifetime)
    {
        if (effectPrefab == null)
            return;

        Vector3 effectPosition = mecha.transform.position;
        effectPosition.y += height;

        GameObject effect = Instantiate(
            effectPrefab,
            effectPosition,
            Quaternion.identity
        );

        Destroy(effect, lifetime);
    }
}
