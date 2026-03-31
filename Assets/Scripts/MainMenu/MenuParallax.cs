using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    [Header("Settings")]
    public float moveAmount = 20f; // How many pixels to move
    public float smoothTime = 0.1f; // How fast it reacts (lower = snapper)

    private Vector2 startPos;
    private Vector2 currentVelocity;

    [Header("Breathing Settings")]
    public float zoomAmount = 0.05f; // How much to zoom in/out
    public float zoomSpeed = 0.5f; // How fast to breathe
    private Vector3 startScale;

    void Start()
    {
        // Remember where the image started
        startPos = GetComponent<RectTransform>().anchoredPosition;
        startScale = GetComponent<RectTransform>().localScale;
    }

    void Update()
    {
        // 1. Get Mouse Position (0 to 1)
        float x = Input.mousePosition.x / Screen.width;
        float y = Input.mousePosition.y / Screen.height;

        // 2. Remap to -1 to 1 (Center is 0)
        float xOffset = (x - 0.5f) * 2; 
        float yOffset = (y - 0.5f) * 2;

        // 3. Calculate Target Position (Move OPPOSITE to mouse)
        Vector2 targetPos = new Vector2(
            startPos.x + (xOffset * -moveAmount), 
            startPos.y + (yOffset * -moveAmount)
        );

        // 4. Smoothly move there
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.SmoothDamp(
            rt.anchoredPosition, 
            targetPos, 
            ref currentVelocity, // Helper variable for SmoothDamp
            smoothTime
        );

        // 5. Breathing Effect (Zoom)
        float scale = Mathf.Sin(Time.time * zoomSpeed) * zoomAmount;
        rt.localScale = startScale + Vector3.one * scale;
    }
}