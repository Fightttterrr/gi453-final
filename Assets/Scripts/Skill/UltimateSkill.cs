using UnityEngine;
using System.Collections;
using System;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class UltimateSkill : SkillBase
{
    public int hitCount = 4;
    public float interval = 1f;
    public Vector2 hitBoxSize = new Vector2(5, 3);
    public LayerMask enemyLayer;
    public GameObject ultimateEffectPrefab; 
    public Transform attackPoint;

    SkillUsageAnalytics analytics;

    void Start()
    {
        analytics = FindObjectOfType<SkillUsageAnalytics>(); // หา script analytics
    }

    protected override void Activate()
    {
        if (analytics != null)
        {
            analytics.UseUltimate(); // ส่งข้อมูลว่าใช้ Ultimate
        }

        StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
       
        player.movement.CanMove = false;
        if (player.combat != null) player.combat.IsUsingUltimate = true;
        player.stats.SetInvincible(hitCount * interval + 1f); 

        int damagePerHit = GetLevelScaledDamage();

        for (int i = 0; i < hitCount; i++)
        {
            // Wait for Animation Event or Timeout
            bool eventTriggered = false;
            Action onTrigger = () => eventTriggered = true;
            
            // Subscribe
            if (player.combat != null) player.combat.OnSkillEventTriggered += onTrigger;

            float timer = 0f;
            float maxWait = 2.0f; // Failsafe timeout

            // Wait loop
            while (!eventTriggered && timer < maxWait)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Unsubscribe
            if (player.combat != null) player.combat.OnSkillEventTriggered -= onTrigger;

            // Perform Hit Logic
            
            // --- Visuals ---
            if (ultimateEffectPrefab != null && attackPoint != null)
            {
                Instantiate(ultimateEffectPrefab, attackPoint.position, attackPoint.rotation);
            }

            //  Move Forward
            float dir = player.animHandler.GetComponentInChildren<SpriteRenderer>().flipX ? 1 : -1; // Assuming default Right? Check PlayerMovement fix earlier. 
            // WAIT - earlier fix said flipX=true is LEFT.
            // If flipX is true (Left), dir should be -1.
            // Code here says: flipX ? 1 : -1. This means flipX=true -> 1 (Right?). 
            // THIS CONTRADICTS MY PREVIOUS FIX.
            // Let's fix this consistency.
            // Previous Fix: flipX=true => Left (-1).
            // So: flipX ? -1 : 1.
            dir = player.animHandler.GetComponentInChildren<SpriteRenderer>().flipX ? -1f : 1f;

            player.transform.Translate(Vector2.right * dir * 1.5f);

            //  Find Enemies
            Collider2D[] enemies = Physics2D.OverlapBoxAll(player.transform.position, hitBoxSize, 0, enemyLayer);

            foreach (var e in enemies)
            {
                // Snap Enemy
                e.transform.position = Vector2.MoveTowards(e.transform.position, player.transform.position + (Vector3.right * dir), 10f);

                // Apply Damage
                Enemy enemyScript = e.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    float stun = (i == hitCount - 1) ? 2f : 0.1f;
                    Vector2 knockback = new Vector2(dir * 1f, 5f);
                    enemyScript.TakeDamage(damagePerHit, knockback, stun); 
                }
            }
        }

        if (player.combat != null) player.combat.IsUsingUltimate = false;
        player.movement.CanMove = true;
    }
}