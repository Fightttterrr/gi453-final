using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public Slider expSlider; // Changed to Slider
    public TextMeshProUGUI expText;

    [Header("References")]
    public PlayerStats playerStats;

    private void Start()
    {
        if (playerStats == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerStats = player.stats;
            }
        }

        if (playerStats != null)
        {
            // Initial update
            UpdateLevelDisplay(playerStats.currentLevel);
            // We need a way to get max XP for current level to init bar properly
            UpdateExpDisplay(playerStats.currentXP, playerStats.GetXPToNextLevel());

            // Subscribe
            playerStats.OnLevelUp += UpdateLevelDisplay;
            playerStats.OnXPChanged += UpdateExpDisplay;
        }
        else
        {
            Debug.LogWarning("LevelUI: PlayerStats not found!");
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= UpdateLevelDisplay;
            playerStats.OnXPChanged -= UpdateExpDisplay;
        }
    }

    private void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }
    }

    private void UpdateExpDisplay(int currentXP, int maxXP)
    {
        if (expSlider != null)
        {
            float progress = (float)currentXP / maxXP;
            expSlider.value = Mathf.Clamp01(progress);
        }

        if (expText != null)
        {
            expText.text = $"{currentXP} / {maxXP}";
        }
    }
}
