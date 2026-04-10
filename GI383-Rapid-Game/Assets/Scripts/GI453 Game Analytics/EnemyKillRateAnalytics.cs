using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class EnemyKillRateAnalytics : MonoBehaviour
{
    private int enemyKillCount = 0;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Analytics EnemyKillRate Ready");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Analytics init failed: " + e.Message);
        }
    }

    // ฆ่าศัตรู
    public void OnEnemyKilled()
    {
        enemyKillCount++;
        Debug.Log("Enemy Killed: " + enemyKillCount);
    }

    // ส่งตอนจบ (ตาย)
    public void SendEnemySummary()
    {
        CustomEvent combatEvent = new CustomEvent("enemy_combat")
        {
            { "enemy_killed", enemyKillCount }
        };

        AnalyticsService.Instance.RecordEvent(combatEvent);

        Debug.Log($"Enemy Summary Sent | Kill: {enemyKillCount}");

        // reset
        enemyKillCount = 0;
    }
}