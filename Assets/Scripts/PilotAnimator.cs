
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Animation States")]
    public string idleAnimation = "Idle";
    public string runAnimation = "Run";

    private int idleHash;
    private int runHash;
    private bool wasMoving;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        idleHash = Animator.StringToHash("Base Layer." + idleAnimation);
        runHash = Animator.StringToHash("Base Layer." + runAnimation);
    }

    void OnEnable()
    {
        wasMoving = false;
    }

    void Start()
    {
        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Update()
    {
        if (animator == null || Keyboard.current == null)
            return;

        Keyboard keyboard = Keyboard.current;

        bool isMoving =
            keyboard.wKey.isPressed ||
            keyboard.aKey.isPressed ||
            keyboard.sKey.isPressed ||
            keyboard.dKey.isPressed;

        int desiredState = isMoving ? runHash : idleHash;

        if (!animator.HasState(0, desiredState))
        {
            Debug.LogWarning(
                "Pilot Animator missing state: " +
                (isMoving ? runAnimation : idleAnimation)
            );
            return;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (!state.fullPathHash.Equals(desiredState) &&
            !animator.IsInTransition(0))
        {
            animator.CrossFade(desiredState, 0.1f, 0);
        }

        if (isMoving != wasMoving)
        {
            Debug.Log(
                "Pilot animation requested: " +
                (isMoving ? "RUN" : "IDLE")
            );

            wasMoving = isMoving;
        }
    }
}
