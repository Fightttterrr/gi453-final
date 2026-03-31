using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Health, XP }
    
    public ItemType type;
    public int amount;
        
    [Header("Magnet Stats (XP Only)")]
    public float magnetRadius = 5f;
    public float magnetSpeed = 8f;
    public float collectDistance = 0.5f;

    [Header("Splash Stats")]
    public float friction = 2f; // Slow down over time
    private Vector2 velocity;
    private bool isSplashing = false;

    [Header("Visuals")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    private Vector3 startPos;
    private float bobOffset;
    private Transform playerTransform;
    private bool isMagnetized = false;

    [Header("Sprites")]
    public Sprite xpSprite;
    public Sprite healthSprite;
    private SpriteRenderer spriteRenderer;

    private Vector3 initialScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
    }

    void Start()
    {
        startPos = transform.position;
        bobOffset = Random.Range(0f, 10f); // Randomize start cycle
        
        // Find player slightly cheaper than Update
        // Find player slightly cheaper than Update
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) 
            {
                playerTransform = p.transform;
            }
            else
            {
                // Fallback if tag is missing
                PlayerMovement pm = FindObjectOfType<PlayerMovement>();
                if (pm != null) playerTransform = pm.transform;
                else Debug.LogWarning($"ItemPickup: Player not found for {gameObject.name}!");
            }
        }
        
        // Initialize visualization if set via inspector
        UpdateVisuals();
    }

    public void SetType(ItemType newType)
    {
        type = newType;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        
        if (type == ItemType.XP && xpSprite != null)
        {
            spriteRenderer.sprite = xpSprite;
            transform.localScale = initialScale;
        }
        else if (type == ItemType.Health && healthSprite != null)
        {
            spriteRenderer.sprite = healthSprite;
            transform.localScale = initialScale * 3f; // Triple size for importance
        }
    }

    public void Splash(Vector2 force)
    {
        velocity = force;
        isSplashing = true;
    }
    
    void Update()
    {
        float dt = Time.deltaTime;

        // 1. Handle Splash Physics
        if (isSplashing)
        {
            transform.Translate(velocity * dt);
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, friction * dt);
            if (velocity == Vector2.zero) isSplashing = false;
            
            // Update startPos for bobbing reference to follow the item
            startPos = transform.position; 
        }

        // 2. Magnet Logic (XP always, Health only if injured)
        if (playerTransform != null && !isSplashing)
        {
            bool shouldMagnetize = false;

            if (type == ItemType.XP)
            {
                shouldMagnetize = true;
            }
            else if (type == ItemType.Health)
            {
                // Only magnetize if player needs health
                PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
                if (stats != null && stats.currentHP < stats.maxHP)
                {
                    shouldMagnetize = true;
                }
            }

            if (shouldMagnetize)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                
                if (dist <= magnetRadius)
                {
                    isMagnetized = true;
                }

                if (isMagnetized)
                {
                    // Fly to player
                    transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * dt);
                    
                    // Collect check (Manual check because Collision is disabled)
                    if (Vector2.Distance(transform.position, playerTransform.position) <= collectDistance)
                    {
                        Collect(playerTransform.GetComponent<PlayerStats>());
                    }
                    return; // Skip bobbing if magnetizing
                }
            }
            else
            {
                // If we were magnetized but condition failed (e.g. healed to full), lose magnetization?
                // Or just keep it? Let's reset for now so it drops if you heal up.
                isMagnetized = false;
            }
        }

        // 3. Visual Bobbing (Only if not moving rapidly)
        if (!isMagnetized && !isSplashing)
        {
            float newY = startPos.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    // Removed OnTriggerEnter2D as we use Distance check now per requirement.
    // Physics collision with Player is ignored via GameManager.

    private void Collect(PlayerStats stats)
    {
        if (stats == null) return;
        
        switch (type)
        {
            case ItemType.Health:
                // Double check usage? 
                // Magnet logic already checked < maxHP, but harmless to call Heal again.
                // If at full health, Heal does nothing but we destroy item?
                // Yes, standard game logic often consumes the item or we add a check here.
                // Given the magnet logic only pulls if Injured, we are safe to consume.
                amount = 1; // FIXED: Healthkit are fixed to heal ONLY 1 HP no matter what
                stats.Heal(amount);
                if (FloatingTextManager.Instance != null)
                    FloatingTextManager.Instance.ShowHeal(amount, stats.transform.position + Vector3.up);
                break;
            case ItemType.XP:
                stats.AddXP(amount);
                break;
        }
        
        Destroy(gameObject);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectDistance);
    }
}
