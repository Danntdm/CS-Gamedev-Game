
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MechaAim : MonoBehaviour
{
    public Camera mainCamera;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Vector3 targetDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Create a horizontal plane at the mech's height.
        Plane aimPlane = new Plane(Vector3.up, transform.position);

        if (aimPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            targetDirection = hitPoint - transform.position;
            targetDirection.y = 0f;
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
