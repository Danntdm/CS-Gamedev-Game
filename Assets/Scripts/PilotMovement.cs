
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PilotMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            movementInput = Vector2.zero;
            return;
        }

        movementInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            movementInput.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            movementInput.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            movementInput.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            movementInput.x += 1f;

        movementInput = Vector2.ClampMagnitude(movementInput, 1f);
    }

    void FixedUpdate()
    {
        Vector3 direction = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );
    }
}
