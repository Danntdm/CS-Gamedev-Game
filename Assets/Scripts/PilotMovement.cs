
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PilotMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator pilotAnimator;

    private Rigidbody rb;
    private Vector2 movementInput;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Automatically find the Animator on the visual child.
        if (pilotAnimator == null)
        {
            pilotAnimator = GetComponentInChildren<Animator>(true);
        }
    }

    void OnEnable()
    {
        movementInput = Vector2.zero;

        if (pilotAnimator != null)
        {
            pilotAnimator.SetFloat(SpeedHash, 0f);
        }
    }

    void OnDisable()
    {
        movementInput = Vector2.zero;

        if (pilotAnimator != null)
        {
            pilotAnimator.SetFloat(SpeedHash, 0f);
        }

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            movementInput = Vector2.zero;
        }
        else
        {
            movementInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed)
                movementInput.y += 1f;

            if (Keyboard.current.sKey.isPressed)
                movementInput.y -= 1f;

            if (Keyboard.current.aKey.isPressed)
                movementInput.x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                movementInput.x += 1f;

            movementInput = Vector2.ClampMagnitude(
                movementInput, 1f
            );
        }

        // 0 = Idle, 1 = Running.
        if (pilotAnimator != null)
        {
            pilotAnimator.SetFloat(
                SpeedHash,
                movementInput.sqrMagnitude > 0.01f ? 1f : 0f
            );
        }
    }

    void FixedUpdate()
    {
        Vector3 direction = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        if (rb.isKinematic)
        {
            rb.MovePosition(
                rb.position +
                direction * moveSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            Vector3 velocity = rb.linearVelocity;

            rb.linearVelocity = new Vector3(
                direction.x * moveSpeed,
                velocity.y,
                direction.z * moveSpeed
            );
        }
    }
}
