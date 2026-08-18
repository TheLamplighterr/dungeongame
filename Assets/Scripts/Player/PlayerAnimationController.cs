using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Animator animator;

    [Header("Animator Parameter Names")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string jumpParam = "Jump";
    [SerializeField] private string landParam = "Land";
    [SerializeField] private string lightAttackParam = "LightAttack";
    [SerializeField] private string heavyAttackParam = "HeavyAttack";
    [SerializeField] private string aimParam = "Aim";
    [SerializeField] private string throwParam = "Throw";
    [SerializeField] private string interactParam = "Interact";
    [SerializeField] private string idleWatchingParam = "IdleWatching";

    private int speedHash;
    private int isGroundedHash;
    private int jumpTriggerHash;
    private int landTriggerHash;
    private int lightAttackTriggerHash;
    private int heavyAttackTriggerHash;
    private int aimBoolHash;
    private int throwTriggerHash;
    private int interactTriggerHash;
    private int idleWatchingTriggerHash;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        CacheHashes();
    }

    private void CacheHashes()
    {
        speedHash = Animator.StringToHash(speedParam);
        isGroundedHash = Animator.StringToHash(groundedParam);
        jumpTriggerHash = Animator.StringToHash(jumpParam);
        landTriggerHash = Animator.StringToHash(landParam);
        lightAttackTriggerHash = Animator.StringToHash(lightAttackParam);
        heavyAttackTriggerHash = Animator.StringToHash(heavyAttackParam);
        aimBoolHash = Animator.StringToHash(aimParam);
        throwTriggerHash = Animator.StringToHash(throwParam);
        interactTriggerHash = Animator.StringToHash(interactParam);
        idleWatchingTriggerHash = Animator.StringToHash(idleWatchingParam);
    }

    private bool HasParameter(int paramHash)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash) return true;
        }
        return false;
    }

    // --- Bewegung & Idle ---
    public void SetSpeed(float speed)
    {
        if (HasParameter(speedHash)) animator.SetFloat(speedHash, speed);
    }

    public void SetGrounded(bool isGrounded)
    {
        if (HasParameter(isGroundedHash)) animator.SetBool(isGroundedHash, isGrounded);
    }

    public void TriggerJump()
    {
        if (HasParameter(jumpTriggerHash)) animator.SetTrigger(jumpTriggerHash);
    }

    public void TriggerLand()
    {
        if (HasParameter(landTriggerHash)) animator.SetTrigger(landTriggerHash);
    }

    public void TriggerIdleWatching()
    {
        if (HasParameter(idleWatchingTriggerHash)) animator.SetTrigger(idleWatchingTriggerHash);
    }

    // --- Kampf & Interaktion ---
    public void TriggerLightAttack()
    {
        if (HasParameter(lightAttackTriggerHash)) animator.SetTrigger(lightAttackTriggerHash);
    }

    public void TriggerHeavyAttack()
    {
        if (HasParameter(heavyAttackTriggerHash)) animator.SetTrigger(heavyAttackTriggerHash);
    }

    public void SetAiming(bool isAiming)
    {
        if (HasParameter(aimBoolHash)) animator.SetBool(aimBoolHash, isAiming);
    }

    public void TriggerThrow()
    {
        if (HasParameter(throwTriggerHash)) animator.SetTrigger(throwTriggerHash);
    }

    public void TriggerInteract()
    {
        if (HasParameter(interactTriggerHash)) animator.SetTrigger(interactTriggerHash);
    }
}