
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PilotAim : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;

    [Header("Rotation")]
    public float rotationSpeed = 15f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Aim along a horizontal plane at the pilot's height.
        Plane groundPlane = new Plane(
            Vector3.up,
            rb.position
        );

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 targetPoint = ray.GetPoint(distance);

        Vector3 direction = targetPoint - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }
}
