using UnityEngine;
using System;
using System.Collections;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class ThrowKnifeSkill : SkillBase
{
    public GameObject knifePrefab;
    public float speed = 20f;

    SkillUsageAnalytics analytics;

    void Start()
    {
        analytics = FindObjectOfType<SkillUsageAnalytics>(); // หา script analytics
    }

    protected override void Activate()
    {
        if (analytics != null)
        {
            analytics.UseThrowKnife();   // ส่งข้อมูลว่าใช้สกิล
        }

        StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        // 1. Lock Movement & Set State
        player.movement.CanMove = false;
        if (player.combat != null) player.combat.IsThrowingKnife = true;

        // 2. Wait for Animation Event (Bridge) where knife leaves hand
        bool eventTriggered = false;
        Action onTrigger = () => eventTriggered = true;

        if (player.combat != null) player.combat.OnSkillEventTriggered += onTrigger;

        float timer = 0f;
        float maxWait = 1.0f; // Short timeout for throw

        while (!eventTriggered && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (player.combat != null) player.combat.OnSkillEventTriggered -= onTrigger;

        // 3. Spawn Knife
        // ตรวจสอบทิศทางจาก SpriteRenderer ของ Player
        float dir = player.animHandler.GetComponentInChildren<SpriteRenderer>().flipX ? -1 : 1;
        
        if (knifePrefab != null)
        {
            GameObject knife = Instantiate(knifePrefab, player.transform.position, Quaternion.identity);
            int dmg = GetLevelScaledDamage();
            // ส่งค่า Damage และทิศทางไปให้มีด
            knife.GetComponent<KnifeProjectile>().Setup(dir, speed, dmg);
        }

        // Wait for Backswing (Animation finish)
        yield return new WaitForSeconds(0.125f);

        // 4. Cleanup
        if (player.combat != null) player.combat.IsThrowingKnife = false;
        player.movement.CanMove = true;
    }
}