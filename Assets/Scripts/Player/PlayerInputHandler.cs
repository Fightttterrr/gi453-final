using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerMovement movement;
    public PlayerCombat combat;
    public SkillManager skillManager;
    private Vector2 moveInput;

    void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (skillManager == null) skillManager = GetComponent<SkillManager>();
    }

    void Update()
    {
        if (movement != null)
        {
            movement.Move(moveInput);
        }
    }

    private enum BufferedInput { None, Jump, Dash, Attack }
    private BufferedInput bufferedInput = BufferedInput.None;

    void OnEnable()
    {
        if (movement != null)
        {
            movement.OnDashEnd += TryExecuteBufferedInput;
            movement.OnLandingEnd += TryExecuteBufferedInput;
        }
        if (combat != null)
        {
            combat.OnAttackEnd += TryExecuteBufferedInput;
        }
    }

    void OnDisable()
    {
        if (movement != null)
        {
            movement.OnDashEnd -= TryExecuteBufferedInput;
            movement.OnLandingEnd -= TryExecuteBufferedInput;
        }
        if (combat != null)
        {
            combat.OnAttackEnd -= TryExecuteBufferedInput;
        }
    }

    private bool IsLocked()
    {
        bool dashing = movement != null && movement.IsDashing;
        bool landing = movement != null && movement.IsLanding;
        bool attacking = combat != null && combat.IsAttacking;
        bool throwing = combat != null && combat.IsThrowingKnife;
        bool isHit = combat != null && combat.stats != null && combat.stats.IsHit;
        bool isDead = combat != null && combat.stats != null && combat.stats.IsDead;

        return dashing || landing || attacking || throwing || isHit || isDead;
    }

    private void TryExecuteBufferedInput()
    {
        // If we are still locked by something else (e.g. finished Dash but immediately Landing?), 
        // we might need to wait unless this event signifies the end of the lock.
        // But usually, one action ends and we are free.
        // However, if we finish Attack but are Landing, IsLocked() might still be true.
        // So we should check IsLocked() again? 
        // If TryExecuteBufferedInput is called via Event, it means one lock released.
        // If another lock persists, we retain the buffer?

        if (IsLocked()) return; // Still locked by something else

        switch (bufferedInput)
        {
            case BufferedInput.Jump:
                movement.Jump();
                break;
            case BufferedInput.Dash:
                movement.Dash();
                break;
            case BufferedInput.Attack:
                if (combat != null) combat.Attack();
                else SendMessage("PerformAttack", SendMessageOptions.DontRequireReceiver);
                break;
            // Removed BufferedInput.Shoot case
        }
        bufferedInput = BufferedInput.None;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (IsLocked())
            {
                bufferedInput = BufferedInput.Jump;
            }
            else if (movement != null)
            {
                movement.Jump();
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            if (IsLocked())
            {
                bufferedInput = BufferedInput.Dash;
            }
            else if (movement != null)
            {
                movement.Dash();
            }
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            if (IsLocked())
            {
                bufferedInput = BufferedInput.Attack;
            }
            else if (combat != null)
            {
                combat.Attack();
            }
            else
            {
                SendMessage("PerformAttack", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /*public void OnShoot(InputValue value)
    {
        if (value.isPressed)
        {
            if (IsLocked())
            {
                bufferedInput = BufferedInput.Shoot;
            }
            else if (combat != null)
            {
                combat.Shoot();
            }
            else
            {
                SendMessage("PerformShoot", SendMessageOptions.DontRequireReceiver);
            }
        }
    }*/
    public void OnSkill1(InputValue value)
    {
        if (value.isPressed && skillManager != null) skillManager.TryUseSkill(1);
    }

    public void OnSkill2(InputValue value)
    {
        if (value.isPressed && skillManager != null) skillManager.TryUseSkill(2);
    }

    public void OnSkill3(InputValue value)
    {
        if (value.isPressed && skillManager != null) skillManager.TryUseSkill(3);

    }
}
