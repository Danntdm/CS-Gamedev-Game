
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MechaAim : MonoBehaviour
{
    public Camera mainCamera;
    public float rotationSpeed = 15f;

    [Header("Mouse Deadzone")]
    public float deadzoneEnterRadius = 85f;
    public float deadzoneExitRadius = 115f;

    public bool IsInDeadzone { get; private set; }
    public Vector3 AimDirection => targetDirection;

    private Rigidbody rb;
    private Vector3 targetDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Start by facing the direction the mecha already faces.
        targetDirection = transform.forward;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.001f)
            targetDirection.Normalize();
    }

    void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // Find where the mecha is on the screen.
        Vector3 mechaScreenPosition =
            mainCamera.WorldToScreenPoint(transform.position);

        if (mechaScreenPosition.z <= 0f)
            return;

        float mouseDistance = Vector2.Distance(
            mousePosition,
            new Vector2(
                mechaScreenPosition.x,
                mechaScreenPosition.y
            )
        );

        // Two thresholds prevent jitter at the boundary.
        float enterRadius = Mathf.Max(0f, deadzoneEnterRadius);
        float exitRadius = Mathf.Max(
            enterRadius,
            deadzoneExitRadius
        );

        if (IsInDeadzone)
        {
            if (mouseDistance >= exitRadius)
                IsInDeadzone = false;
        }
        else
        {
            if (mouseDistance <= enterRadius)
                IsInDeadzone = true;
        }

        // Inside the deadzone, remember the previous aim.
        if (IsInDeadzone)
            return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Horizontal plane at the mecha's height.
        Plane aimPlane =
            new Plane(Vector3.up, transform.position);

        if (aimPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            Vector3 newDirection =
                hitPoint - transform.position;

            newDirection.y = 0f;

            if (newDirection.sqrMagnitude > 0.001f)
                targetDirection = newDirection.normalized;
        }
    }

    void FixedUpdate()
    {
        if (targetDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(targetDirection);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }
}
