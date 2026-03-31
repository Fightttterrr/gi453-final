using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public WaveUI waveUI;

    [Header("Wave Configuration (ตั้งค่าพื้นฐาน)")]
    [Tooltip("ระยะเวลาพักระหว่าง Wave (วินาที)")]
    public float timeBetweenWaves = 5f;
    [Tooltip("เวลาสุ่มเกิดศัตรู (วินาที) - ค่าต่ำสุด")]
    public float spawnIntervalMin = 0.2f;
    [Tooltip("เวลาสุ่มเกิดศัตรู (วินาที) - ค่าสูงสุด")]
    public float spawnIntervalMax = 1.5f;

    [Header("Time Limit Settings")]
    [Tooltip("เวลาเริ่มต้นสำหรับ Wave 1")]
    public float baseWaveTime = 30f;
    [Tooltip("เวลาที่เพิ่มขึ้นในแต่ละ Wave")]
    public float waveTimeIncrement = 5f;

    [Header("Endless Difficulty Settings (การเพิ่มความยาก)")]
    [Tooltip("จำนวนศัตรูเริ่มต้นใน Wave 1")]
    public int baseEnemyCount = 5;
    [Tooltip("จำนวนศัตรูที่จะเพิ่มขึ้นในแต่ละ Wave (เช่น ใส่ 2 คือเพิ่มทีละ 2 ตัว)")]
    public int enemyCountIncrement = 2;

    [Header("Stat Multipliers (ตัวคูณความเก่ง)")]
    [Tooltip("เปอร์เซ็นต์เลือดที่เพิ่มขึ้นต่อ Wave (เช่น 0.1 คือเพิ่ม 10%)")]
    public float hpIncreasePercentage = 0.1f;
    [Tooltip("เปอร์เซ็นต์ความเร็วที่เพิ่มขึ้นต่อ Wave (เช่น 0.05 คือเพิ่ม 5%)")]
    public float speedIncreasePercentage = 0.05f;

    private int currentWave = 0;
    public int CurrentWave => currentWave;

    void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (waveUI == null)
        {
            waveUI = FindFirstObjectByType<WaveUI>();
        }
        
        StartCoroutine(StartNextWave());
    }

    private bool isSpawning = false;

    IEnumerator StartNextWave()
    {
        currentWave++;
        Debug.Log($"--- Starting Wave {currentWave} ---");

        if (waveUI != null)
        {
            waveUI.UpdateWaveCount(currentWave);
            waveUI.HideCenterText();
        }

        // Countdown 3, 2, 1
        if (waveUI != null)
        {
            for (int i = 3; i > 0; i--)
            {
                waveUI.ShowCenterText(i.ToString());
                yield return new WaitForSeconds(1f);
            }
            waveUI.HideCenterText();
        }

        // Calculate enemies for this round
        int enemiesToSpawn = baseEnemyCount + ((currentWave - 1) * enemyCountIncrement);

        // Calculate stat multipliers
        float currentHpMultiplier = 1f + ((currentWave - 1) * hpIncreasePercentage);
        float currentSpeedMultiplier = 1f + ((currentWave - 1) * speedIncreasePercentage);

        // Calculate time limit for this round
        float currentWaveTime = baseWaveTime + ((currentWave - 1) * waveTimeIncrement);

        // Start spawning in parallel
        isSpawning = true;
        StartCoroutine(SpawnEnemiesRoutine(enemiesToSpawn, currentHpMultiplier, currentSpeedMultiplier));

        // Wait Loop: Ends if Time runs out OR (All enemies are dead AND spawning is finished)
        float timer = currentWaveTime;
        bool waveCleared = false;

        while (timer > 0 && !waveCleared)
        {
            timer -= Time.deltaTime;
            
            // Check enemies count
            int enemyCount = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
            
            // Winning Condition: No enemies left AND spawning is finished
            if (enemyCount == 0 && !isSpawning)
            {
                waveCleared = true;
            }

            // Update UI
            if (waveUI != null)
            {
                waveUI.UpdateTimer(timer);

                // Last 5 seconds countdown
                if (timer <= 5.0f && timer > 0)
                {
                    // Adding 1 to ceil makes it show 5, 4, 3, 2, 1 correctly
                    waveUI.ShowCenterText(Mathf.CeilToInt(timer).ToString());
                }
                else
                {
                   // Ensure center text is hidden if we are not in the last 5 seconds (e.g. at start of wave)
                   if (timer > 5.0f) 
                   {
                        waveUI.HideCenterText();
                   }
                }
            }

            yield return null;
        }

        Debug.Log($"Wave {currentWave} Cleared or Time Up!");

        // Wave End Logic
        if (waveUI != null)
        {
            waveUI.UpdateTimer(0);
            waveUI.ShowCenterText("Wave Incoming");
        }

        // Wait before next wave
        yield return new WaitForSeconds(timeBetweenWaves);

        // Start next wave
        StartCoroutine(StartNextWave());
    }

    IEnumerator SpawnEnemiesRoutine(int enemiesToSpawn, float hpMultiplier, float speedMultiplier)
    {
         for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (spawner != null)
            {
                GameObject enemyObj = spawner.SpawnEnemy();

                if (enemyObj != null)
                {
                    Enemy enemyScript = enemyObj.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        enemyScript.hp *= hpMultiplier;
                        enemyScript.moveSpeed *= speedMultiplier;
                    }
                }
            }
            float randomInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(randomInterval);
        }
        isSpawning = false;
    }
}
