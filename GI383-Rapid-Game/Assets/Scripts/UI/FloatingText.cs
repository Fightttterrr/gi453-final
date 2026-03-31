using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float lifeTime = 1.0f;
    private float floatSpeed = 2.0f;
    private Color startColor;
    
    private Vector3 initialScale;
    private Vector3 targetScale;
    
    private float timer;

    public void Init(string text, Color color, float sizeScale = 1.0f)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();
        
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = 6; // Base size
        textMesh.alignment = TextAlignmentOptions.Center;
        
        startColor = color;
        timer = 0;
        
        // Start smaller and pop up
        transform.localScale = Vector3.one * 0.5f * sizeScale;
        initialScale = transform.localScale;
        targetScale = Vector3.one * 1.2f * sizeScale; // Pop slightly larger
        
        // TextMeshPro settings for "Worldspace" look
        textMesh.sortingOrder = 50; // Ensure on top of sprites
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        float progress = timer / lifeTime;

        if (progress >= 1.0f)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Float Up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. Scale Animation (Pop up then settle)
        if (progress < 0.2f)
        {
            // Scale up fast
            float scaleProgress = progress / 0.2f;
             transform.localScale = Vector3.Lerp(initialScale, targetScale, scaleProgress);
        }
        else
        {
            // Slow scale down return or just stay? 
             // Let's keep it big or slowly shrink.
             transform.localScale = Vector3.Lerp(targetScale, targetScale * 0.8f, (progress - 0.2f) / 0.8f);
        }

        // 3. Fade Out (Last 50%)
        if (progress > 0.5f)
        {
            float fadeProgress = (progress - 0.5f) / 0.5f;
            float alpha = Mathf.Lerp(1.0f, 0.0f, fadeProgress);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }
}
