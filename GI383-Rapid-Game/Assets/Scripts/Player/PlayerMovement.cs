using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerStats stats;
    public SpriteRenderer spriteRenderer;

    [Header("Movement Stats")]
    public float speed = 5f;
    public float jumpForce = 10f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Feel")]
    public float jumpStartupTime = 0.05f;
    public float landingLagTime = 0.1f;
    
    [Header("Physics Checks")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    
    // States
    public bool IsDashing { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsLanding { get; private set; }
    public bool CanMove { get; set; } = true;

    private bool canDash = true;
    private int enemyLayerID;

    // Events
    public event Action<bool> OnGroundedChanged;
    public event Action OnJumpStart;
    public event Action OnDashStart;
    public event Action OnDashEnd;

    public event Action OnLandingEnd;
    public event Action OnLand; // New event for landing impact
    public event Action OnRunStart; // New event for starting to run

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Auto-find references if not assigned
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        enemyLayerID = LayerMask.NameToLayer("Enemy");
    }


    void Start()
    {
        // Subscribe to stats level up
        if (stats != null)
        {
            stats.OnLevelUp += OnLevelUp;
            UpdateStats();
        }
    }

    void OnDestroy()
    {
        if (stats != null) stats.OnLevelUp -= OnLevelUp;
    }

    private void OnLevelUp(int newLevel)
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (stats != null)
        {
            // Use current calculated stats from PlayerStats
            // Only update if they are greater than 0 (initialized)
            if (stats.CurrentMoveSpeed > 0) speed = stats.CurrentMoveSpeed;
            if (stats.CurrentJumpForce > 0) jumpForce = stats.CurrentJumpForce;
            if (stats.CurrentDashSpeed > 0) dashSpeed = stats.CurrentDashSpeed;
        }
    }

    private bool ignoreCollisionJump;
    private bool isCollisionIgnored;

    void Update()
    {
        CheckGround();
        UpdateCollisionState();
    }

    private void UpdateCollisionState()
    {
        bool shouldIgnore = (stats != null && stats.IsInvincible()) || IsDashing || ignoreCollisionJump;
        
        if (shouldIgnore != isCollisionIgnored)
        {
            IgnoreEnemyCollision(shouldIgnore);
            isCollisionIgnored = shouldIgnore;
        }
    }

    public void Move(Vector2 input)
    {
        if (!CanMove || IsDashing || IsLanding || IsJumping) 
        {
            // If we are strictly locked, don't move X. 
            // NOTE: IsJumping corresponds to 'isJumpStarting' in original code (startup frames).
            // Normal air movement is usually allowed unless designed otherwise.

            // FIX: Ensure we actually STOP if CanMove is false (e.g. attacking on ground)
            // But we might want to keep Y velocity (gravity)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Apply movement
        rb.linearVelocity = new Vector2(input.x * speed, rb.linearVelocity.y);
        
        // Flip sprite
        if (input.x != 0)
        {
            // Assuming default sprite faces Right
            // If moving left (x < 0), flipX should be true
            if (spriteRenderer != null) spriteRenderer.flipX = input.x < 0; 
        }


        // Trigger Run Event
        bool isRunning = input.x != 0 && Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded;

        if (isRunning && !wasRunning)
        {
             OnRunStart?.Invoke();
        }
        wasRunning = isRunning;
    }

    private bool wasRunning;
    private float lastRunTime; // Kept just in case, or remove if unused. Let's keep it clean and remove if not needed. But let's leave var if referenced elsewhere (it's private so likely safe to remove but I'll leave definitions alone to minimize diff if possible, actually wait, I defined it last time).
    // Actually cleanliness is better. I'll replace the block to use wasRunning.

    public void Jump()
    {
        if (isGrounded && !IsDashing && !IsJumping && !IsLanding)
        {
            StartCoroutine(JumpCoroutine());
        }
    }

    public void Dash()
    {
        if (canDash && !IsDashing && !IsJumping && !IsLanding)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        
        if (isGrounded != wasGrounded)
        {
            OnGroundedChanged?.Invoke(isGrounded);
            
            // Landing Logic
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                StartCoroutine(LandingCoroutine());
            }
            OnGroundedChanged?.Invoke(isGrounded);
            
            // Landing Logic
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                OnLand?.Invoke(); // Fire Landing Event
                StartCoroutine(LandingCoroutine());
            }
        }
        wasGrounded = isGrounded;
    }

    public bool IsGrounded() => isGrounded;

    private IEnumerator JumpCoroutine()
    {
        IsJumping = true;
        OnJumpStart?.Invoke();
        
        yield return new WaitForSeconds(jumpStartupTime);
        
        ignoreCollisionJump = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        IsJumping = false;
        
        // Note: We don't restore collision here immediately, original maintained it while in air? 
        // Original: "IgnoreEnemyCollision(true)" inside Jump. Restored in Landing.
    }

    private IEnumerator LandingCoroutine()
    {
        IsLanding = true;
        
        // Restore collision
        ignoreCollisionJump = false;

        yield return new WaitForSeconds(landingLagTime);
        IsLanding = false;
        OnLandingEnd?.Invoke();
    }

    private IEnumerator DashCoroutine()
    {
        IsDashing = true;
        canDash = false;
        CanMove = false; // Disable movement explicitely
        OnDashStart?.Invoke();
        
        if (stats != null) stats.SetInvincible(dashDuration + 0.35f); // Duration + buffer (User modified this recently)

        float originalScale = rb.gravityScale;
        rb.gravityScale = 0;
        
        float dir = 1f;
        // If flipX is true (Left), direction is -1. If flipX is false (Right), direction is 1.
        if (spriteRenderer != null) dir = spriteRenderer.flipX ? -1f : 1f;

        StartCoroutine(ShowGhosts());

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            
            // Quadratic Deceleration: v = v0 * (1-t)^2
            float decelerationFactor = (1f - t) * (1f - t);
            float currentDashSpeed = dashSpeed * decelerationFactor;
            
            rb.linearVelocity = new Vector2(dir * currentDashSpeed, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalScale;
        rb.linearVelocity = Vector2.zero;
        
        IsDashing = false;
        CanMove = true; // Re-enable movement
        OnDashEnd?.Invoke();
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void TriggerKnockUp(float force)
    {
        // Cancel Y velocity for consistent jump
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    
    private void IgnoreEnemyCollision(bool ignore)
    {
         if (enemyLayerID != -1)
         {
             Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerID, ignore);
         }
    }

    private IEnumerator ShowGhosts()
    {
        while (IsDashing)
        {
            GameObject ghost = new GameObject("Ghost");
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.localScale;

            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = spriteRenderer.sprite;
            //ghostSr.flipX = spriteRenderer.flipX; // Inherited from visual
            // Actually new ghost copies sprite renderer props manually
            ghostSr.flipX = spriteRenderer.flipX; 
            ghostSr.sortingLayerID = spriteRenderer.sortingLayerID;
            ghostSr.sortingOrder = spriteRenderer.sortingOrder - 1;
            
            // Simple fade script
            ghost.AddComponent<GhostFade>();

            yield return new WaitForSeconds(0.05f);
        }
    }
}
