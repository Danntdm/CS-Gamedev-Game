
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MechaMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 9f;

    private Rigidbody rb;
    private Vector2 movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        ResetMovement();
    }

    void OnDisable()
    {
        ResetMovement();
    }

    public void ResetMovement()
    {
        movementInput = Vector2.zero;

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 direction = Vector3.ClampMagnitude(
            new Vector3(movementInput.x, 0f, movementInput.y),
            1f
        );

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime
        );
    }
}
