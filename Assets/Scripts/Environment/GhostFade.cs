using UnityEngine;

public class GhostFade : MonoBehaviour
{
    private SpriteRenderer sr;
    public float fadeSpeed = 1f;
    public float lifeTime = 0.5f;
    public float startAlpha = 0.5f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Set initial alpha
        Color color = sr.color;
        color.a = startAlpha;
        sr.color = color;
        
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (sr != null)
        {
            Color color = sr.color;
            color.a = Mathf.Max(0, color.a - (fadeSpeed * Time.deltaTime));
            sr.color = color;
        }
    }
}
