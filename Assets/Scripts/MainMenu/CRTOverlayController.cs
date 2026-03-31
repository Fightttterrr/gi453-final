using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CRTOverlayController : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2.0f;
    public float minAberration = 0.002f;
    public float maxAberration = 0.006f;

    [Header("Noise Settings")]
    public float noiseSpeed = 15.0f;
    public float noiseAmount = 0.001f;

    private Image overlayImage;
    private Material crtMaterial;

    void Awake()
    {
        overlayImage = GetComponent<Image>();
        
        // Create an instance of the material so we don't modify the asset
        if (overlayImage.material != null)
        {
            crtMaterial = new Material(overlayImage.material);
            overlayImage.material = crtMaterial;
        }
    }

    void Update()
    {
        if (crtMaterial == null) return;

        // 1. Slow Pulse
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
        float baseAberration = Mathf.Lerp(minAberration, maxAberration, pulse);

        // 2. Fast Jitter/Noise (Glitch feeling)
        float noise = (Mathf.PerlinNoise(Time.time * noiseSpeed, 0) - 0.5f) * noiseAmount;

        float finalAberration = Mathf.Clamp(baseAberration + noise, 0f, 0.05f);

        crtMaterial.SetFloat("_AberrationAmount", finalAberration);
    }

    void OnDestroy()
    {
        if (crtMaterial != null)
        {
            Destroy(crtMaterial);
        }
    }
}
