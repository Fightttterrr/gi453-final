using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarUI : MonoBehaviour
{
    [Header("Heart Assets")]
    [Tooltip("Prefab for the heart UI element (must have Image component)")]
    public GameObject heartPrefab;
    [Tooltip("Container for heart instances (Horizontal Layout Group). Defaults to this object if null.")]
    public Transform heartContainer;

    [Header("Heart Sprites")]
    [Tooltip("Sprite for a full heart (2 HP)")]
    public Sprite fullHeartSprite;
    
    [Tooltip("Sprite for a half heart (1 HP)")]
    public Sprite halfHeartSprite;
    
    [Tooltip("Sprite for an empty heart (0 HP)")]
    public Sprite emptyHeartSprite;

    [Header("Heart UI Elements")]
    [Tooltip("List of instantiated hearts. Index 0 is the Highest HP heart (Rightmost).")]
    public List<Image> heartImages = new List<Image>();

    [Header("Player Reference")]
    [Tooltip("Reference to the Player component")]
    public Player player;

    [Header("Profile Picture")]
    [Tooltip("Image component for the player profile picture")]
    public Image profileImage;

    [Tooltip("Sprite for Normal status (Health >= 50%)")]
    public Sprite normalProfileSprite;

    [Tooltip("Sprite for Pain status (20% < Health < 50%)")]
    public Sprite painProfileSprite;

    [Tooltip("Sprite for Near Death status (Health <= 20%)")]
    public Sprite nearDeathProfileSprite;

    [Range(0f, 1f)]
    [Tooltip("Threshold for Pain status (0.5 means 50%)")]
    public float painThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Threshold for Near Death status (0.2 means 20%)")]
    public float nearDeathThreshold = 0.2f;

    private int maxHP; // Cached max HP
    private Vector2 defaultCellSize; // Stores the initial cell size logic


    private void Start()
    {
        if (heartContainer == null) heartContainer = transform;
        
        // Capture default cell size
        GridLayoutGroup grid = heartContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            defaultCellSize = grid.cellSize;
            // Apply requested constraint
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
        }

        if (fullHeartSprite == null || halfHeartSprite == null || emptyHeartSprite == null)
        {
            Debug.LogWarning("HealthBarUI: One or more heart sprites are not assigned.");
        }
        
        // Delay initialization to ensure PlayerStats and Layout are ready
        StartCoroutine(InitializeRoutine());
    }

    private System.Collections.IEnumerator InitializeRoutine()
    {
        // Wait for end of frame to ensure Layout is built (for RectTransform dimensions)
        yield return new WaitForEndOfFrame();

        if (player == null) player = FindFirstObjectByType<Player>();

        // Wait until PlayerStats is valid and initialized
        while (player == null || player.stats == null || player.stats.maxHP <= 0)
        {
             if (player == null) player = FindFirstObjectByType<Player>();
             yield return null; 
        }

        if (player != null && player.stats != null)
        {
            // Subscribe to events
            player.stats.OnHealthChanged += UpdateHealth;
            player.stats.OnLevelUp += HandleLevelUp;

            // Initial Setup
            UpdateMaxHP(player.stats.maxHP);
            UpdateHealth(player.stats.currentHP);
        }
        else
        {
            // Fallback
            maxHP = 10;
        }
    }

    private void OnDestroy()
    {
        if (player != null && player.stats != null)
        {
            player.stats.OnHealthChanged -= UpdateHealth;
            player.stats.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleLevelUp(int level)
    {
        // When level increases, stats are recalculated, so we update MaxHP and refresh UI
        if (player != null && player.stats != null)
        {
            UpdateMaxHP(player.stats.maxHP);
            UpdateHealth(player.stats.currentHP);
        }
    }

    public void Initialize(int maxHealth)
    {
        UpdateMaxHP(maxHealth);
    }
    
    public void UpdateMaxHP(int newMaxHP)
    {
        this.maxHP = newMaxHP;
        
        // Calculate needed hearts (2 HP per heart)
        // User requested "suppose to be 2 to Instantiate not 1".
        // Interpreted as: Only instantiate for full 2 HP chunks. Integer division.
        int requiredHearts = maxHP / 2;
        
        if (heartPrefab == null)
        {
            // If no prefab, we can't instantiate. Rely on existing inspector-assigned hearts.
            return;
        }

        // Adjust heart count
        int currentCount = heartImages.Count;
        int diff = requiredHearts - currentCount;

        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                CreateHeart();
            }
        }
        else if (diff < 0)
        {
            for (int i = 0; i < Mathf.Abs(diff); i++)
            {
                RemoveHeart();
            }
        }

        ResizeHeartsToFit(requiredHearts);
        
        // Ensure accurate initial state
        if (player != null && player.stats != null)
        {
            UpdateHealth(player.stats.currentHP);
        }
    }

    private void ResizeHeartsToFit(int heartCount)
    {
        if (heartContainer == null) return;
        
        GridLayoutGroup grid = heartContainer.GetComponent<GridLayoutGroup>();
        RectTransform containerRect = heartContainer.GetComponent<RectTransform>();
        
        if (grid != null && containerRect != null)
        {
            // We assume horizontal loading for a health bar
            float containerWidth = containerRect.rect.width;
            
            // Safety check: if width is 0 (e.g. layout not built), use a default or skip
            if (containerWidth <= 0)
            {
               // Try to fallback to parent or just return to avoid 0 size
               return; 
            }

            float spacingX = grid.spacing.x;
            float paddingLeft = grid.padding.left;
            float paddingRight = grid.padding.right;
            
            float availableWidth = containerWidth - paddingLeft - paddingRight;
            
            // Equation: (width * count) + (spacing * (count - 1)) = available
            
            // With fixed columns (7), we want to fit up to 7 items in the width.
            // If heartCount < 7, fit heartCount. If >= 7, fit 7.
            int columns = Mathf.Min(heartCount, 7);
            if (columns < 1) columns = 1;

            float totalSpacing = spacingX * Mathf.Max(0, columns - 1);
            float maxCellWidth = (availableWidth - totalSpacing) / columns;
            
            // Determine new size
            // We want to shrink if needed, but not grow larger than default
            // Also ensure we don't go below a minimum decent size (e.g. 1) to avoid invisible hearts
            if (maxCellWidth < 1) maxCellWidth = 10; // Fallback min size
            
            float newSizeX = Mathf.Min(defaultCellSize.x, maxCellWidth);
            float newSizeY = newSizeX; // Assume square ratio or maintain ratio
            
            // If default was not square, we should maintain aspect ratio
            if (defaultCellSize.x > 0)
            {
                float ratio = defaultCellSize.y / defaultCellSize.x;
                newSizeY = newSizeX * ratio;
            }

            grid.cellSize = new Vector2(newSizeX, newSizeY);
        }
    }

    private void CreateHeart()
    {
        if (heartPrefab == null || heartContainer == null) return;

        GameObject newHeart = Instantiate(heartPrefab, heartContainer);
        Image heartImg = newHeart.GetComponent<Image>();
        if (heartImg != null)
        {
            // We insert at 0 because our logic uses Index 0 as the "Highest HP" heart.
            // Visually: The new heart is appended to the container (Rightmost).
            // Logic: Index 0 (High HP) -> Child N (Rightmost).
            // So the New Heart (which is High HP) should be at Index 0.
            heartImages.Insert(0, heartImg);
            
            // Set default sprite
            heartImg.sprite = emptyHeartSprite;
        }
    }
    
    private void RemoveHeart()
    {
        if (heartImages.Count > 0)
        {
            // Remove High HP heart (Index 0 is Highest HP / Rightmost)
            // This happens if MaxHP decreases.
            Image heartToRemove = heartImages[0];
            heartImages.RemoveAt(0);
            Destroy(heartToRemove.gameObject);
        }
    }

    /// <summary>
    /// Updates the health bar display based on current HP
    /// </summary>
    public void UpdateHealth(int currentHP)
    {
        // Use effective MaxHP based on instantiated hearts to align ranges correctly
        // If real MaxHP is 11, we have 5 hearts (Effective 10).
        // If we used 11, Heart 0 would be range 10-11. 10 HP would show as Half.
        // We want 10 HP to show as Full. So use Effective Max 10.
        int effectiveMaxHP = heartImages.Count * 2;
        
        // Update Profile Picture Logic
        // We use the 'maxHP' field which caches the true player MaxHP, or effectiveMaxHP if more appropriate?
        // Usually Profile Pic should depend on Real HP %.
        // Let's use the maxHP we cached from the player stats, or fallback to effective if issues.
        int referenceMaxHP = (this.maxHP > 0) ? this.maxHP : effectiveMaxHP;
        if (referenceMaxHP <= 0) referenceMaxHP = 1; // Prevent division by zero

        float healthPercent = (float)currentHP / referenceMaxHP;
        
        if (profileImage != null)
        {
            if (healthPercent <= nearDeathThreshold && nearDeathProfileSprite != null)
            {
                profileImage.sprite = nearDeathProfileSprite;
            }
            else if (healthPercent <= painThreshold && painProfileSprite != null)
            {
                profileImage.sprite = painProfileSprite;
            }
            else if (normalProfileSprite != null)
            {
                profileImage.sprite = normalProfileSprite;
            }
        }

        // Clamp currentHP to effective Max for display purposes
        // (If player has 11 HP, treat as 10)
        int displayHP = Mathf.Clamp(currentHP, 0, effectiveMaxHP);

        for (int i = 0; i < heartImages.Count; i++)
        {
            // Heart 0: Represents the 'Top' of the HP bar (Highest values)
            int heartIndex = i; 
            int maxHPForThisHeart = effectiveMaxHP - (heartIndex * 2);

            if (displayHP >= maxHPForThisHeart)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else if (displayHP == maxHPForThisHeart - 1 && displayHP > 0)
            {
                heartImages[i].sprite = halfHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
}
