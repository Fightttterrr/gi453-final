using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;
    public float spawnDistanceMin = 10f;
    public float spawnDistanceMax = 15f;
    public float spawnHeightOffset = 1.0f;

    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    
    private Camera mainCamera;

    [Header("Ground Check Layer")]
    public LayerMask groundLayer;

    // ANALYTICS
    SkillUsageAnalytics analytics;


    void Start()
    {
        mainCamera = Camera.main;

        // ANALYTICS
        analytics = FindObjectOfType<SkillUsageAnalytics>();

        if (player == null)
        {
            Player p = FindFirstObjectByType<Player>();
            if (p != null) player = p.transform;
        }

        // Auto-assign Ground layer if possible
        if (groundLayer.value == 0)
        {
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            if (groundLayerIndex != -1)
            {
                groundLayer = 1 << groundLayerIndex;
            }
        }
    }

    
    public GameObject SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No Enemy Prefabs assigned!");
            return null;
        }
        if (player == null)
        {
            Player p = FindFirstObjectByType<Player>();
            if (p != null) player = p.transform;

            if (player == null)
            {
                // Debug.LogError("EnemySpawner: Player not found!");
                return null;
            }
        }

        // Try to get a valid spawn position
        if (TryGetSpawnPosition(out Vector2 spawnPos))
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            // return ตัวที่เสกออกมา
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // เพิ่ม Analytics
            if (analytics != null)
                analytics.AddEnemyEncounter();

            return enemy;
        }
        else
        {
            return null;
        }
    }

    bool TryGetSpawnPosition(out Vector2 spawnPosition)
    {
        
        spawnPosition = Vector2.zero;
        if (mainCamera == null) mainCamera = Camera.main;

        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float buffer = 5f;
        float minDistance = (camWidth / 2f) + buffer;
        float extraDistance = Random.Range(0, spawnDistanceMax - spawnDistanceMin);
        float distance = minDistance + extraDistance;

        float leftX = mainCamera.transform.position.x - distance;
        float rightX = mainCamera.transform.position.x + distance;

        bool leftValid = CheckGround(leftX, out float leftY);
        bool rightValid = CheckGround(rightX, out float rightY);

        if (leftValid && rightValid)
        {
            if (Random.value > 0.5f) spawnPosition = new Vector2(leftX, leftY);
            else spawnPosition = new Vector2(rightX, rightY);
            return true;
        }
        else if (leftValid)
        {
            spawnPosition = new Vector2(leftX, leftY);
            return true;
        }
        else if (rightValid)
        {
            spawnPosition = new Vector2(rightX, rightY);
            return true;
        }

        return false;
    }

    bool CheckGround(float xPos, out float yPos)
    {
        
        yPos = 0;
        float rayStartY = mainCamera.transform.position.y + mainCamera.orthographicSize + 5f;
        Vector2 rayOrigin = new Vector2(xPos, rayStartY);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 50f, groundLayer);

        if (hit.collider != null)
        {
            float yDiff = Mathf.Abs(hit.point.y - player.position.y);
            if (yDiff > 5f)
            {
                return false;
            }
            yPos = hit.point.y + spawnHeightOffset;
            return true;
        }
        return false;
    }
}