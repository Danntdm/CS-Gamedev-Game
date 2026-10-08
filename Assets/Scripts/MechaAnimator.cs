
using UnityEngine;

public class MechaAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody mechaRb;

    [Header("Movement")]
    public float movementThreshold = 0.5f;
    public float transitionTime = 0.15f;

    [Header("Animations")]
    public string idleAnimation = "Idle_gunMiddle_AR";
    public string runAnimation = "Run_gunMiddle_AR";
    public string shootingAnimation = "Shoot_AutoShot_AR";

    [Header("Shooting")]
    public float shootingAnimationDuration = 0.25f;

    private Vector3 previousPosition;
    private Vector3 movementVelocity;

    private string currentAnimation = "";
    private float shootingTimer;

    void Start()
    {
        if (mechaRb != null)
            previousPosition = mechaRb.position;

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void FixedUpdate()
    {
        if (mechaRb == null)
            return;

        Vector3 currentPosition = mechaRb.position;

        movementVelocity =
            (currentPosition - previousPosition) /
            Time.fixedDeltaTime;

        movementVelocity.y = 0f;
        previousPosition = currentPosition;
    }

    void Update()
    {
        if (animator == null || mechaRb == null)
            return;

        if (shootingTimer > 0f)
            shootingTimer -= Time.deltaTime;

        bool isMoving =
            movementVelocity.magnitude > movementThreshold;

        bool isShooting =
            shootingTimer > 0f;

        string nextAnimation = idleAnimation;

        // Running takes priority over shooting.
        if (isMoving)
        {
            nextAnimation = runAnimation;
        }
        else if (isShooting)
        {
            nextAnimation = shootingAnimation;
        }

        if (nextAnimation != currentAnimation)
        {
            animator.CrossFade(
                nextAnimation,
                transitionTime
            );

            currentAnimation = nextAnimation;
        }
    }

    public void PlayShootAnimation()
    {
        shootingTimer = shootingAnimationDuration;
    }
}
