using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;

    public float hp = 10f; // Converted High Priority to Float

    [Header("Detection & State")]
    public float detectionRange = 5f;
    public float giveUpRange = 8f; // Range to stop chasing
    public EnemyState currentState = EnemyState.Idle;
    public enum EnemyState { Idle, Chasing, Attacking, Stunned, KnockedBack }
    
    [Header("Stun Settings")]
    private float stunTimer = 0f;
    private Color originalColor;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback Settings")] 
    public float knockbackForce = 5f; 
    public float knockbackDuration = 0.2f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f; 
    public float attackCooldown = 1.5f; 
    public float attackDelay = 0.5f; // Delay before damage is dealt
    private float nextAttackTime = 0f;  
    private bool isAttackingFlag = false; // Internal flag for coroutine
    public int damage = 10;
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    

    [Header("Drops")]
    public GameObject itemDropPrefab;
    public int minXP = 10;
    public int maxXP = 20;

    [Header("VFX")]
    public ParticleSystem hitParticlePrefab;
    public ParticleSystem deathEffectPrefab;

    private Animator anim;

    // private bool isKnockedBack = false; // Removed in favor of State

    private Transform player;
    private Player playerScript; // Cached reference
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;
    private Coroutine knockbackCoroutine;
    private System.Collections.Generic.List<Collider2D> ignoredColliders = new System.Collections.Generic.List<Collider2D>();
    private WaveManager waveManager;


    void Start()
    {

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
        
        GameObject pObj = GameObject.FindWithTag("Player");
        if (pObj == null) 
             pObj = FindFirstObjectByType<Player>()?.gameObject;
             
        if (pObj != null)
        {
            player = pObj.transform;
            playerScript = pObj.GetComponent<Player>();
        }



        waveManager = FindFirstObjectByType<WaveManager>();
    }

    void FixedUpdate()
    {

        if (transform.position.y < -20f) 
        {
            Destroy(gameObject); 
            return;
        }
        // Global State Machine
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Attacking:
                HandleAttacking(); // Usually empty, waiting for coroutine
                break;
            case EnemyState.Stunned:
                HandleStunned();
                break;
            case EnemyState.KnockedBack:
                // Physics handles movement here. We mostly want to wait.
                // Ensure no conflicting velocity applications
                break; 
        }

        // Anti-float logic (gravity helper)
        // Only apply if grounded. If flying (knocked back), let physics work.
        if (currentState != EnemyState.KnockedBack && IsGrounded() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        // Check if fallen out of map
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    // --- State Handlers ---

    void HandleIdle()
    {
        StopMoving();
        
        if (player != null && playerScript != null)
        {
            // Detection Logic
            if (!playerScript.IsInvisible)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
            }
        }
    }

    void HandleChasing()
    {
        if (player == null) 
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Check if player became invisible
        if (playerScript.IsInvisible)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > giveUpRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (dist <= attackRange)
        {
            StopMoving();
            // Ready to attack?
            if (Time.time >= nextAttackTime)
            {
                currentState = EnemyState.Attacking;
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
        if (IsGrounded())
        {
            MoveTowardsPlayer();
        }
        }
    }

    void HandleAttacking()
    {
        // Movement is stopped. Logic inside AttackRoutine handles transition back to Idle/Chasing.
        // This state primarily prevents other logic from overriding the attack.
    }

    void HandleStunned()
    {
        StopMoving();

        if (anim != null)
        {
            anim.SetFloat("Speed", 0);
                       
        }
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            currentState = EnemyState.Idle; // Or Chasing
            spriteRenderer.color = originalColor;
        }
    }

    // --- Actions ---

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x != 0)
        {
            bool faceRight = direction.x > 0;
            spriteRenderer.flipX = faceRight;
            
            // Flip attack point logic
            Vector3 currentPos = attackPoint.localPosition;
            if (faceRight) currentPos.x = Mathf.Abs(currentPos.x);
            else currentPos.x = -Mathf.Abs(currentPos.x);
            attackPoint.localPosition = currentPos;
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void ApplyStun(float duration)
    {
        if (hp <= 0) return;
        if (currentState == EnemyState.KnockedBack) return; // Don't interrupt knockback? Or should we? Let's respect physics first.
        
        stunTimer = duration;
        currentState = EnemyState.Stunned;
        
        // Visual feedback
        spriteRenderer.color = Color.yellow;
        
        if (isAttackingFlag)
        {
            StopAllCoroutines(); // Cancel attack
            isAttackingFlag = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttackingFlag = true;
        currentState = EnemyState.Attacking;


        if (anim != null) anim.SetTrigger("isAttacking");
        yield return new WaitForSeconds(attackDelay);

        if (currentState != EnemyState.Stunned && currentState != EnemyState.KnockedBack && hp > 0) 
        {

            // Hit Check
            if (player != null && !playerScript.IsInvisible)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= attackRange * 1.2f) 
                {
                    playerScript.TakeDamage(damage);
                    Debug.Log("Enemy Hit Player!");
                }
            }
        }
        
        isAttackingFlag = false;
        nextAttackTime = Time.time + attackCooldown;
        
        // Return to Chasing (or Idle will handle next frame)
        if (currentState != EnemyState.Stunned && currentState != EnemyState.KnockedBack)
            currentState = EnemyState.Chasing; 
    }

    // --- Health & Damage ---

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public void TakeDamage(float damage)
    {
        // Default behavior if called without knockback (e.g. environment trap)
        // Apply zero knockback or a small default.
        TakeDamage(damage, Vector2.zero, 0.1f);
    }
    
    public void TakeDamage(float damage, bool applyKnockback)
    {

        
        if (applyKnockback)
        {
             TakeDamage(damage, Vector2.zero, 0.1f);
        }
        else
        {
             // Just damage, no knockback routine
             hp -= damage;
             ShowDamageText(damage); 
             SpawnHitVFX();
             
             // Visual Flash only
             StartCoroutine(DamageFlashRoutine(0.2f));

             if (hp <= 0) Die();
        }
    }

    private System.Collections.IEnumerator DamageFlashRoutine(float duration)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(duration);
        // Restore color only if not stunned (Stun manages its own color)
        if (currentState != EnemyState.Stunned)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void ShowDamageText(float dmg)
    {
         if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowDamage(dmg, transform.position + Vector3.up * 0.5f);
    }

    private void SpawnHitVFX()
    {
        if (hitParticlePrefab == null || myCollider == null) return;
        Bounds b = myCollider.bounds;
        float px = Random.Range(b.min.x, b.max.x);
        float minY = Mathf.Max(b.min.y, b.center.y - 0.5f);
        float py = Random.Range(minY, b.max.y);
        Vector3 spawnPos = new Vector3(px, py, transform.position.z);
        ParticleSystem ps = Instantiate(hitParticlePrefab, spawnPos, Quaternion.identity);
        float randomScale = Random.Range(0.8f, 1.1f); 
        ps.transform.localScale = Vector3.one * randomScale;
        Destroy(ps.gameObject, Mathf.Max(ps.main.duration, 0.5f));
    }

    public void TakeDamage(float damage, Vector2 knockbackVector, float duration)
    {
        if(hp > 0)
        {
        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(KnockedBackRoutine(knockbackVector, duration));

        hp -= damage;
        ShowDamageText(damage);

        // Hit VFX
        SpawnHitVFX();
            
            // Random Scale (0.8 - 1.2 approx range to cover 0.11 if it was a typo for 1.1, assuming 0.8-1.1)

        }

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isAttackingFlag) StopAllCoroutines();
        
        // Death VFX
        if (deathEffectPrefab != null)
        {
            // Ensure visualization by bringing it forward in Z (assuming Camera is at -10)
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f);
            ParticleSystem ps = Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
            
            ps.Play(); // Force play
            Debug.Log("Playing Death Effect");
            
            Destroy(ps.gameObject, Mathf.Max(ps.main.duration, 2f)); // Increased safety buffer
        }
        
        // 1. EXP Drop (3-4 Items, 100% chance each to spawn, Splash out)
        if (itemDropPrefab != null)
        {
            int dropCount = Random.Range(5, 7); // 3 or 4
            
            int wave = (waveManager != null) ? waveManager.CurrentWave : 1;
            // Calculus logic: Exponential Growth
            // Formula: Base * (1.1 ^ (Wave-1))
            float growthFactor = 1.1f;
            float multiplier = Mathf.Pow(growthFactor, wave - 1);
            
            for (int i = 0; i < dropCount; i++)
            {
                GameObject xp = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
                ItemPickup pickup = xp.GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    pickup.SetType(ItemPickup.ItemType.XP);

                    // Base calculation
                    float baseVal = Random.Range(minXP, maxXP + 1);
                    float scaledVal = baseVal * multiplier;
                    
                    // Random variation: 0.7 - 1.1
                    float randomVar = Random.Range(0.7f, 1.1f);
                    
                    pickup.amount = Mathf.Max(1, Mathf.RoundToInt(scaledVal * randomVar)); // Ensure at least 1
                    
                    // Splash Logic
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    float randomForce = Random.Range(3f, 6f);
                    pickup.Splash(randomDir * randomForce);
                }
            }
            
            // 2. Health Drop (Calculated Chance)
            SpawnHealthDrop();
        }

        Destroy(gameObject);
    }

    void SpawnHealthDrop()
    {
        if (playerScript == null || playerScript.stats == null) return;
        if (itemDropPrefab == null) return;

        // Player Health Factor
        float hpPercent = (float)playerScript.stats.currentHP / playerScript.stats.maxHP;
        float baseChance = 0.1f; // 10% base
        float lowHpBonus = (1f - hpPercent) * 0.4f; // Up to +40% if HP is 0
        
        // Enemy Factor
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        float enemyFactor = 1f / (1f + (enemyCount * 0.2f)); 

        // Healthkit Present on Field Factor
        ItemPickup[] allItems = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        int healthKitCount = 0;
        foreach (var item in allItems) if (item.type == ItemPickup.ItemType.Health) healthKitCount++;
        
        float itemFactor = 1f / (1f + (healthKitCount * 0.5f)); // Drastically reduce if kits exist

        float finalChance = (baseChance + lowHpBonus) * enemyFactor * itemFactor;
        
        // Cap
        if (finalChance > 0.8f) finalChance = 0.8f;

        if (Random.value < finalChance)
        {
            GameObject hpDrop = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
            ItemPickup pickup = hpDrop.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.SetType(ItemPickup.ItemType.Health);
                pickup.amount = 1; 
                
                // Also splash health a bit so it doesn't look static
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                pickup.Splash(randomDir * 3f);
            }
        }
    }

    IEnumerator KnockedBackRoutine(Vector2 force, float stunDuration)
    {
        currentState = EnemyState.KnockedBack;


        // Ignore collision with other enemies to prevent getting stuck
        if (myCollider != null)
        {
            // Find all colliders on the same layer (assuming Enemy layer)
            // Use a generous radius to catch anyone we might fly through
            Collider2D[] others = Physics2D.OverlapCircleAll(transform.position, 5f); // 5f check radius
            foreach (var col in others)
            {
                if (col != myCollider && !col.isTrigger && col.gameObject.layer == gameObject.layer)
                {
                    Physics2D.IgnoreCollision(myCollider, col, true);
                    ignoredColliders.Add(col);
                }
            }
        }
        
        // Fix Floating Issue:
        // Only reset Y velocity if there's a significant upwards force (launching).
        // Otherwise (horizontal shove), keep existing Y velocity to let gravity work naturally.
        if (force.y > 0.1f)
        {
            rb.linearVelocity = Vector2.zero; // Full reset for launch
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Reset X only
        }

        rb.AddForce(force, ForceMode2D.Impulse);
        
        // spriteRenderer.color = Color.red; // Managed by specific state or FlashRoutine if unified
        spriteRenderer.color = Color.red;

        // Brief delay to allow physics to lift the enemy off the ground if applicable
        yield return new WaitForSeconds(0.1f);

        // Fall/Fly loop: Wait until we touch the ground again
        float failsafeTimer = 0f;
        while (!IsGrounded() && failsafeTimer < 3f) // 3s failsafe in case they fall continually
        {
            failsafeTimer += Time.deltaTime;
            yield return null;
        }

        

        // Landed! Now apply Stun
        currentState = EnemyState.Idle; // Reset state temporarily so ApplyStun works
        spriteRenderer.color = originalColor; 
        
        // Restore collisions
        if (myCollider != null)
        {
            foreach (var otherCol in ignoredColliders)
            {
                if (otherCol != null) Physics2D.IgnoreCollision(myCollider, otherCol, false);
            }
            ignoredColliders.Clear();
        }

        ApplyStun(stunDuration);
            
        knockbackCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, attackArea);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
