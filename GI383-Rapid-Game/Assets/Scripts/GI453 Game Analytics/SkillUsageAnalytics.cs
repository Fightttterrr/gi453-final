using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class SkillUsageAnalytics : MonoBehaviour
{
    public int enemyCount = 0;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Analytics Usaged Skills Ready");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Analytics init failed: " + e.Message);
        }
    }

    //Event บันทึกตอนเจอศัตรู
    public void AddEnemyEncounter()
    {
        enemyCount++;

        AnalyticsService.Instance.RecordEvent("enemy_encounter");

        Debug.Log("Enemy Encountered: " + enemyCount);
    }

    // ใช้สกิลต่าง ๆ
    public void UseInvisible()
    {
        RecordSkill("Invisible");
    }

    public void UseThrowKnife()
    {
        RecordSkill("ThrowKnife");
    }

    public void UseUltimate()
    {
        RecordSkill("Ultimate");
    }

    // Event การใช้สกิล
    void RecordSkill(string skillName)
    {
        CustomEvent skillEvent = new CustomEvent("skill_used")
        {
            { "skill_name", skillName }
        };

        AnalyticsService.Instance.RecordEvent(skillEvent);

        Debug.Log("Skill Used: " + skillName);
    }
}