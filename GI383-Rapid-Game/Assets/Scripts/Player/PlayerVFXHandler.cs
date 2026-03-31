using UnityEngine;

public class PlayerVFXHandler : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerMovement movement;
    public PlayerStats stats;
    private Collider2D playerCollider;

    [Header("Effects")]
    public ParticleSystem dustEffect;
    public ParticleSystem hitParticle;
    [Header("Level Up")]
    public GameObject levelUpVFXPrefab;

    void Start()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.OnJumpStart += PlayDust;
            movement.OnLand += PlayDust;
            movement.OnRunStart += PlayDust;
        }

        if (stats == null) stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.OnDamageTaken += PlayHitEffect;
            stats.OnLevelUp += OnLevelUp;
        }

        playerCollider = GetComponent<Collider2D>();
    }

    void OnDestroy()
    {
        if (movement != null)
        {
            movement.OnJumpStart -= PlayDust;
            movement.OnLand -= PlayDust;
            movement.OnRunStart -= PlayDust;
        }

        if (stats != null)
        {
            stats.OnDamageTaken -= PlayHitEffect;
            stats.OnLevelUp -= OnLevelUp;
        }
    }

    private void PlayDust()
    {
        if (dustEffect != null)
        {
            dustEffect.Play();
        }
    }

    private void PlayHitEffect()
    {
        if (hitParticle != null && playerCollider != null)
        {
            Bounds b = playerCollider.bounds;
            float px = Random.Range(b.min.x, b.max.x);
            // "random position should be above than -0.5 of collider" -> Use Center.y - 0.5 as min (clamped to bounds)
            float minY = Mathf.Max(b.min.y, b.center.y - 0.5f);
            float py = Random.Range(minY, b.max.y);
            Vector3 spawnPos = new Vector3(px, py, transform.position.z);

            ParticleSystem ps = Instantiate(hitParticle, spawnPos, Quaternion.identity);
            
            // Random Scale
            float randomScale = Random.Range(0.8f, 1.1f);
            ps.transform.localScale = Vector3.one * randomScale;
            
            Destroy(ps.gameObject, Mathf.Max(ps.main.duration, 0.5f));
        }
    }

    private void OnLevelUp(int newLevel)
    {
        if (levelUpVFXPrefab != null)
        {
            StartCoroutine(PlayLevelUpFX());
        }
    }

    private System.Collections.IEnumerator PlayLevelUpFX()
    {
        // Instantiate above player
        Vector3 spawnPos = transform.position + new Vector3(0, -0.4f, 0); 
        GameObject vfx = Instantiate(levelUpVFXPrefab, spawnPos, Quaternion.identity, transform);
        
        // Ensure it follows player (parented) or stays? "Play when only Player level up".
        // Usually level up follows.

        yield return new WaitForSeconds(1f); // Duration

        if (vfx != null) Destroy(vfx);
    }
}
