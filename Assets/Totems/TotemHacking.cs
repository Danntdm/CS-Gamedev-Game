using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class TotemHacking : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PilotEjection pilotEjection;
    [SerializeField] private ProgressionManager progression;

    [Header("Hacking")]
    [SerializeField, Min(0.1f)] private float hackSeconds = 3f;
    [SerializeField, Min(0.1f)] private float interactionRange = 5.5f;

    private HackableTotem[] totems;
    private PlayerHealth pilot;
    private PlayerHealth mecha;
    private HackableTotem nearbyTotem;
    private float heldSeconds;
    private int hackedCount;
    private bool pilotOutside;
    private bool playerCanInteract;
    private bool rewardUnavailable;
    private GUIStyle labelStyle;

    void Awake()
    {
        totems = GetComponentsInChildren<HackableTotem>();
        if (pilotEjection != null)
        {
            if (pilotEjection.pilot != null)
                pilot = pilotEjection.pilot.GetComponent<PlayerHealth>();
            if (pilotEjection.mecha != null)
                mecha = pilotEjection.mecha.GetComponent<PlayerHealth>();
        }

        if (pilot == null || mecha == null || progression == null)
        {
            Debug.LogWarning("Totem hacking needs the pilot, mecha, and progression manager.", this);
            enabled = false;
        }
    }

    void Update()
    {
        pilotOutside = pilot != null && pilot.gameObject.activeInHierarchy;
        PlayerHealth controlledCharacter = pilotOutside ? pilot : mecha;
        playerCanInteract = controlledCharacter != null && controlledCharacter.isActiveAndEnabled &&
            !controlledCharacter.IsDead() && Time.timeScale > 0f;

        HackableTotem closest = playerCanInteract
            ? FindNearestTotem(controlledCharacter.transform.position)
            : null;

        if (nearbyTotem != closest)
        {
            nearbyTotem = closest;
            ResetHold();
        }

        Keyboard keyboard = Keyboard.current;
        if (nearbyTotem == null || !pilotOutside || keyboard == null || !keyboard.fKey.isPressed)
        {
            ResetHold();
            return;
        }

        if (rewardUnavailable)
            return;

        heldSeconds += Time.deltaTime;
        if (heldSeconds < hackSeconds)
            return;

        if (progression == null || !progression.TryQueueUpgrade())
        {
            rewardUnavailable = true;
            heldSeconds = 0f;
            return;
        }

        nearbyTotem.MarkHacked();
        hackedCount++;
        nearbyTotem = null;
        ResetHold();
    }

    private void ResetHold()
    {
        heldSeconds = 0f;
        rewardUnavailable = false;
    }

    private HackableTotem FindNearestTotem(Vector3 position)
    {
        HackableTotem nearest = null;
        float nearestDistance = interactionRange * interactionRange;

        foreach (HackableTotem totem in totems)
        {
            if (totem == null || !totem.isActiveAndEnabled || totem.IsHacked)
                continue;

            float distance = (totem.transform.position - position).sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearest = totem;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    void OnGUI()
    {
        if (!playerCanInteract || totems.Length == 0 ||
            Time.timeScale <= 0f || Event.current.type != EventType.Repaint)
            return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }

        Rect count = new Rect(Screen.width - 182f, 12f, 170f, 32f);
        GUI.Box(count, GUIContent.none);
        GUI.Label(count, $"Totems hacked: {hackedCount}/{totems.Length}", labelStyle);

        if (nearbyTotem == null)
            return;

        string prompt = "Press E to exit the mecha";
        if (pilotOutside)
        {
            int percent = Mathf.FloorToInt(100f * heldSeconds / hackSeconds);
            prompt = rewardUnavailable ? "Upgrade menu unavailable" : $"Hold F to hack ({percent}%)";
        }

        float width = Mathf.Min(320f, Screen.width - 24f);
        Rect notice = new Rect((Screen.width - width) / 2f, Screen.height * 0.72f, width, 36f);
        GUI.Box(notice, GUIContent.none);
        GUI.Label(notice, prompt, labelStyle);
    }
}
