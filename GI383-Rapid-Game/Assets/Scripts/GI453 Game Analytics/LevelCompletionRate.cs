using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class LevelCompletionRate : MonoBehaviour
{
    private int levelStartCount = 0;
    private int levelCompleteCount = 0;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Analytics LevelCompletionRate Ready");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Analytics init failed: " + e.Message);
        }
    }

    // เริ่มด่าน
    public void OnLevelStart()
    {
        levelStartCount++;
        Debug.Log("Level Started: " + levelStartCount);
    }

    // ผ่านด่าน
    public void OnLevelComplete()
    {
        levelCompleteCount++;
        Debug.Log("Level Completed: " + levelCompleteCount);
    }

    // ส่งตอนจบ session (เช่น ตาย)
    public void SendLevelSummary()
    {
        CustomEvent levelEvent = new CustomEvent("level_progress")
        {
            { "level_start", levelStartCount },
            { "level_complete", levelCompleteCount }
        };

        AnalyticsService.Instance.RecordEvent(levelEvent);

        Debug.Log("Sent Level Progress");

        // reset
        levelStartCount = 0;
        levelCompleteCount = 0;
    }
}
