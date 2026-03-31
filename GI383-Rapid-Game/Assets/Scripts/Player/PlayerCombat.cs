using UnityEngine;
using System.Collections;
using System;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    public LayerMask enemyLayers;
    // Removed attackEffectPrefab
    
    [Header("Combat Stats")]
    public float attackCooldown = 0.5f;
    public float knockbackForceX = 5f;
    public float knockbackForceY = 0f;
    public float stunDuration = 0.2f; // Stun duration on enemy AFTER landing
    public float attackDuration = 0.2f; // Player animation lock duration

    [Header("Weapons (Deprecated/Secondary)")]
    // Removed knifeWeapon
    
    // Dependencies
    public PlayerStats stats;
    private PlayerMovement movement;

    [Header("States")]
    public bool IsAttacking { get; private set; }
    public bool IsThrowingKnife { get; set; } // Set by ThrowKnifeSkill
    public bool IsUsingUltimate { get; set; } // Set by UltimateSkill

    // Events
    public event Action OnAttackStart;
    public event Action OnAttackEnd;
    // Removed OnShootStart, OnShootEnd
    public event Action OnSkillEventTriggered; // Bridge Event for External Skills

    private float nextAttackTime = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Handle Attack Direction Visuals (AttackPoint rotation)
        if (attackPoint != null && movement != null && movement.spriteRenderer != null)
        {
            // PlayerMovement: flipX = true (Left), flipX = false (Right - default)
            
            // If flipX is true (Left), dir is -1. Else 1.
            float dir = movement.spriteRenderer.flipX ? -1f : 1f;
            
            // Flip Position (X) based on local relative
            Vector3 pos = attackPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * dir;
            attackPoint.localPosition = pos;

            // Flip Rotation (Y) - 0 is Right, 180 is Left
            Vector3 rot = attackPoint.localEulerAngles;
            rot.y = movement.spriteRenderer.flipX ? 180f : 0f;
            attackPoint.localEulerAngles = rot;
        }
    }

    public void Attack()
    {
        if (CanAttack())
        {
            StartCoroutine(AttackCoroutine());
        }
    }



    private bool CanAttack()
    {
        bool isGrounded = movement != null && movement.IsGrounded();
        bool isHit = stats != null && stats.IsHit;
        
        // Block attack if:
        // 1. Cooldown not ready
        // 2. Already acting
        // 3. Dashing/Landing
        // 4. In Air (Not Grounded)
        // 5. Stunned (IsHit)
        return Time.time >= nextAttackTime 
            && !IsAttacking 
            && !IsThrowingKnife
            && !movement.IsDashing 
            && !movement.IsLanding
            && isGrounded // Must be on ground to attack
            && !isHit;    // Cannot attack while stunned
    }

    // Removed CanShoot

    private IEnumerator AttackCoroutine()
    {
        IsAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        // Optional: Stop movement during attack? If desired.
        if (movement != null) movement.CanMove = false;
        
        OnAttackStart?.Invoke();
        
        // Start visuals immediately
        PerformAttackVisuals();

        // FAILSAFE: If no animation event is set up, fallback to damage after a small delay? 
        // User requested "Set Event", so we assume they WILL set it.
        // However, if they don't, the attack does nothing. 
        // For now, we strictly respect the requested architecture: Damage is triggered by event.

        yield return new WaitForSeconds(attackDuration);
        
        if (movement != null) movement.CanMove = true;
        IsAttacking = false;
        OnAttackEnd?.Invoke();
    }

    private void PerformAttackVisuals()
    {
         if (attackPoint == null) return;
         // Removed attackEffectPrefab instantiation
    }

    // THIS METHOD MUST BE CALLED BY ANIMATION EVENT
    public void TriggerMeleeAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint is not assigned in PlayerCombat!");
            return;
        }

        // 2. Detect Enemies (AoE)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

        // 3. Apply Damage & Knockback
        float damage = stats != null ? stats.attackDamage : 10f;

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Calculate Knockback Direction (Away from player/attackPoint)
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                
                // Use explicit X/Y forces
                float directionX = Mathf.Sign(dir.x);
                Vector2 knockbackVector = new Vector2(directionX * knockbackForceX, knockbackForceY);
                
                enemy.TakeDamage(damage, knockbackVector, stunDuration);
                Debug.Log($"Hit {enemy.name} for {damage} damage! KB: {knockbackVector}, Stun: {stunDuration}");
            }
        }
    }

    // Bridge Method for External Skills (Animation Event calls this)
    public void TriggerSkillEvent()
    {
        OnSkillEventTriggered?.Invoke();
    }

    // Removed ShootCoroutine

    // Called by InputHandler via SendMessage or direct call
    public void PerformAttack() => Attack();
    // Removed PerformShoot
    
    public void TurnWeapons(bool facingRight)
    {
        // Removed knifeWeapon.Turn
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
    public float GetAttackCooldownProgress()
    {
        if (Time.time >= nextAttackTime) return 0f;
        
        float remaining = nextAttackTime - Time.time;
        // Clamp between 0 and 1 just in case
        return Mathf.Clamp01(remaining / attackCooldown);
    }
}
