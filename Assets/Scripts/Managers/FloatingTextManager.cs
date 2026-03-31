using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Settings")]
    public GameObject textPrefab;
    
    [Header("Colors")]
    public Color damageColor = Color.red;
    public Color healColor = Color.green;
    public Color levelUpColor = Color.blue; // Cyan/Blue

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: Keep across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamage(float damage, Vector3 position)
    {
        // Round to int for clean numbers, or keep float if decimal needed. usually Int is better for arcade games.
        // Input is float, let's show one decimal if < 10 maybe? Or just Int. User example was "-3"
        int displayVal = Mathf.RoundToInt(damage);
        CreateText($"-{displayVal}", damageColor, position, 1.0f);
    }

    public void ShowHeal(int amount, Vector3 position)
    {
        CreateText($"+{amount}", healColor, position, 1.0f);
    }

    public void ShowLevelUp(Vector3 position)
    {
        // Level up is special, maybe bigger?
        CreateText("Level Up!", levelUpColor, position, 1.5f);
    }

    private void CreateText(string content, Color color, Vector3 position, float scale)
    {
        GameObject go;
        if (textPrefab != null)
        {
            go = Instantiate(textPrefab, position, Quaternion.identity);
        }
        else
        {
            // Fallback: Create from scratch
            go = new GameObject("FloatingText_Fallback");
            go.transform.position = position;
        }

        FloatingText ft = go.GetComponent<FloatingText>();
        if (ft == null) ft = go.AddComponent<FloatingText>();
        
        ft.Init(content, color, scale);
    }
}
