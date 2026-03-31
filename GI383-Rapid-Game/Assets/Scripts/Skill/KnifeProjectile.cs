using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    private float direction;
    private float speed;
    private int damage;

    public void Setup(float dir, float spd, int dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        Destroy(gameObject, 5f); 
    }

    void Awake()
    {
        SetupTrail();
    }

    void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = gameObject.AddComponent<TrailRenderer>();

        trail.time = 0.2f;
        trail.startWidth = 0.2f; // Slightly thinner for "dash" feel
        trail.endWidth = 0.0f;
        
        // Use a safe shader usually available. If URP, this might fallback or need explicit "Universal Render Pipeline/Particles/Unlit"
        // But "Particles/Standard Unlit" or "Sprites/Default" usually Render. 
        // Let's try "Sprites/Default" again but ensures Order is high.
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trail.material = trailMat;
        
        // Gradient
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 1f, 1f, 0.5f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        trail.colorGradient = gradient;
        
        trail.sortingLayerName = "Default"; 
        trail.sortingOrder = 5; // Positive value to render in front of background/enemy
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ใช้ Tag หรือ Layer ตาม Enemy.cs ของคุณ
        if (other.CompareTag("Enemy"))
        {
            // เรียกฟังก์ชัน TakeDamage ของศัตรู แบบไม่ Stun / Knockback
            other.GetComponent<Enemy>()?.TakeDamage(damage, false);
            // *ไม่ Destroy* ตัวมีด เพื่อให้ทะลุศัตรูไปเลยตามโจทย์
        }
    }
}