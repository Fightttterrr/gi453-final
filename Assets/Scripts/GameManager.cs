using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public HealthBarUI healthBarUI;
    public DeathSequenceController deathSequenceController;
    public GameObject deathPanel; // Fallback if no sequence controller

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null) player = FindFirstObjectByType<Player>();
        if (healthBarUI == null) healthBarUI = FindFirstObjectByType<HealthBarUI>();
        if (deathSequenceController == null) deathSequenceController = FindFirstObjectByType<DeathSequenceController>(FindObjectsInactive.Include);
        
        StartCoroutine(InitializeRoutine());

        ConfigurePhysics();
    }

    private System.Collections.IEnumerator InitializeRoutine()
    {
        // Wait for end of frame to ensure all other Starts have run
        yield return new WaitForEndOfFrame();

        // Ensure Player reference is found
        while (player == null)
        {
             player = FindFirstObjectByType<Player>();
             yield return null;
        }

        // Wait until PlayerStats is valid (optional, but good for consistency)
        while (player.stats == null)
        {
            yield return null;
        }

        if (player != null && player.stats != null)
        {
            // Subscribe to events
            player.stats.OnDeath += HandleDeath;
            
            // UI initialization is now handled by HealthBarUI itself
            Debug.Log("GameManager: Successfully subscribed to Player events.");
        }
    }

    private void ConfigurePhysics()
    {
        int itemLayer = LayerMask.NameToLayer("Item");
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");

        if (itemLayer == -1 || playerLayer == -1 || groundLayer == -1)
        {
            Debug.LogWarning("GameManager: One or more layers (Item, Player, Ground) are missing in Project Settings!");
            return;
        }

        // Loop through all 32 possible layers
        for (int i = 0; i < 32; i++)
        {
            // If the layer is NOT Ground, ignore collision with Item
            // (We ignore Player now too, per requirement "Ignore Everything except Ground")
            
            if (i != groundLayer)
            {
                Physics2D.IgnoreLayerCollision(itemLayer, i, true);
            }
            else
            {
                // Ensure Ground collides
                Physics2D.IgnoreLayerCollision(itemLayer, i, false);
            }
        }
    }

    void OnDestroy()
    {
        if (player != null && player.stats != null)
        {
            player.stats.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Debug.Log("GameManager: Player Died.");
        
        if (deathSequenceController != null)
        {
            deathSequenceController.StartDeathSequence();
        }
        else if (deathPanel != null)
        {
             deathPanel.SetActive(true);
             Time.timeScale = 0f;
        }
    }
}
