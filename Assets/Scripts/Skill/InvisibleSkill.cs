using UnityEngine;
using System.Collections;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class InvisibleSkill : SkillBase
{
    public float duration = 4f;

    SkillUsageAnalytics analytics;

    void Start()
    {
        analytics = FindObjectOfType<SkillUsageAnalytics>(); // หา script analytics
    }

    protected override void Activate()
    {
        if (analytics != null)
        {
            analytics.UseInvisible(); // บอก analytics ว่าใช้สกิลแล้ว
        }

        StartCoroutine(InvisibleRoutine());
    }

    private IEnumerator InvisibleRoutine()
    {
       
        player.stats.SetInvincible(duration);
        player.IsInvisible = true;

        
        var sr = player.animHandler.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f); // จางลง

    
        yield return new WaitForSeconds(duration);

        
        sr.color = originalColor;
        player.IsInvisible = false;
    }
}