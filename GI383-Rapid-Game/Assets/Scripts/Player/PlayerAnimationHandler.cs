using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    [Header("Dependencies")]
    public Animator anim;
    public PlayerMovement movement;
    public PlayerCombat combat;
    public PlayerStats stats;

    void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (stats != null && stats.IsDead)
        {
            anim.SetBool("IsDie", true);
            // Disable all other states
            anim.SetBool("IsTakeDamage", false);
            anim.SetBool("IsJump", false);
            anim.SetBool("IsAttack", false);
            anim.SetBool("IsThrowKnife", false);
            anim.SetBool("IsDash", false);
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsUltimate", false);
            return;
        }

        if (stats != null && stats.IsHit)
        {
            anim.SetBool("IsTakeDamage", true);
            anim.SetBool("IsJump", false);
            anim.SetBool("IsAttack", false);
            // Removed IsShoot
            anim.SetBool("IsDash", false);
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", false);
            return;
        }
        else
        {
            anim.SetBool("IsTakeDamage", false);
        }

        bool isAttacking = combat != null && combat.IsAttacking;
        bool isThrowing = combat != null && combat.IsThrowingKnife;
        bool isDashing = movement != null && movement.IsDashing;
        bool isGrounded = movement != null && movement.IsGrounded();
        bool isUltimate = combat != null && combat.IsUsingUltimate;

        anim.SetBool("IsAttack", isAttacking);
        anim.SetBool("IsThrowKnife", isThrowing);
        anim.SetBool("IsDash", isDashing);
        anim.SetBool("IsUltimate", isUltimate);

        // Priority Logic similar to Player.cs
        if (isAttacking || isDashing || isUltimate || isThrowing)
        {
            anim.SetBool("IsJump", false);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsIdle", false);
        }
        else
        {
            // Pass Vertical Velocity for Jump/Fall transition
            if (movement.GetComponent<Rigidbody2D>() != null)
            {
                anim.SetFloat("VerticalVelocity", movement.GetComponent<Rigidbody2D>().linearVelocity.y);
            }
            anim.SetBool("IsJump", !isGrounded);

            if (isGrounded)
            {
                // Check velocity for movement state
                float velocityX = movement.GetComponent<Rigidbody2D>().linearVelocity.x;
                bool isMoving = Mathf.Abs(velocityX) > 0.1f;
                anim.SetBool("IsWalk", isMoving);
                anim.SetBool("IsIdle", !isMoving);
            }
            else
            {
                // Ensure other states are off while jumping
                anim.SetBool("IsWalk", false);
                anim.SetBool("IsIdle", false);
            }
        }
    }
    
    // Player.cs logic was:
    // anim.SetBool("IsAttack", isAttacking);
    // ...
    
    // I should probably subscribe to events to set Triggers, or poll states for Bools.
    // Given the previous code used SetBool, I will stick to Polling Bools for states that have duration.

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine splitColorRoutine;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        // If still null, try finding in children
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (stats != null)
        {
            stats.OnDamageTaken += HandleDamageVisuals;
        }
    }

    void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDamageTaken -= HandleDamageVisuals;
        }
    }

    private void HandleDamageVisuals()
    {
        if (splitColorRoutine != null) StopCoroutine(splitColorRoutine);
        splitColorRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private System.Collections.IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        // 1. Turn Red
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        // 2. Turn White
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        // 3. Flicker while invincible
        while (stats != null && stats.IsInvincible())
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle visibility
            yield return new WaitForSeconds(0.1f);
        }

        // Ensure we end up visible and correct color
        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
        splitColorRoutine = null;
    }
}
