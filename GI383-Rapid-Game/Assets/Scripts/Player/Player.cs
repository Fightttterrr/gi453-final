using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerAnimationHandler))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(SkillManager))]
public class Player : MonoBehaviour
{
    // Facade for backward compatibility and central access
    
    [Header("Components")]
    public PlayerStats stats;
    public PlayerMovement movement;
    public PlayerInputHandler inputHandler;
    public PlayerAnimationHandler animHandler;
    public PlayerCombat combat;
    public SkillManager skillManager;

    [Header("References")]
    public CameraFollow camFollow;

    // Backward Compatibility Properties
    public int HP 
    { 
        get => stats != null ? stats.currentHP : 0; 
        private set { if (stats != null) stats.currentHP = value; } // Try to avoid setting direct
    }

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
        inputHandler = GetComponent<PlayerInputHandler>();
        animHandler = GetComponent<PlayerAnimationHandler>();
        combat = GetComponent<PlayerCombat>();
        skillManager = GetComponent<SkillManager>();
        
        // Find external references if missing
        if (camFollow == null) camFollow = Camera.main.GetComponent<CameraFollow>();
    }

    void Start()
    {
        // Setup Event Links
        if (stats != null)
        {
            stats.OnDamageTaken += HandleDamageTaken;
            // stats.InitializeStats(); // Force init if needed, often Stats does it itself in its Start
            // But if we want to ensure order:
            stats.InitializeStats();
        }
    }

    void OnDestroy()
    {
         if (stats != null)
        {
            stats.OnDamageTaken -= HandleDamageTaken;
        }
    }

    [Header("Settings")]
    public float knockUpForce = 5f;

    // State
    public bool IsInvisible { get; set; } = false;

    // --- Event Handlers ---

    private void HandleDamageTaken()
    {
        if (movement != null)
        {
            movement.TriggerKnockUp(knockUpForce);
        }

        if (camFollow != null)
        {
            camFollow.TriggerShake(0.2f, 0.3f);
        }
        // Hit stop?
        StartCoroutine(HitStopRoutine());
    }

    // --- Backward Compatibility ---

    public void TakeDamage(int damage)
    {
        if (stats != null)
        {
            stats.TakeDamage(damage);
        }
    }
    
    // Compatibility methods for InputSystem used by PlayerInput component 
    // IF PlayerInput is sending messages to "Player" GameObject, it targets all components.
    // So PlayerInputHandler.OnMove will catch it. 
    // We don't need to duplicate OnMove here unless PlayerInput component specifically looks for this class type (unlikely).
    // SendMessage targets all MonoBehaviours.

    // Helper coroutine
    private System.Collections.IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.1f); // hitStopDuration
        Time.timeScale = 1f;
    }
}