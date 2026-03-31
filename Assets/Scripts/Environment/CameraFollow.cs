using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Settings")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;
    
    [Header("Bounds")]
    public bool enableBounds = true;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    private Camera cam;
    private float shakeTimer;
    private float shakeMagnitude;

    private Vector3 logicalPosition; // The position without shake

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Initialize logical position
        logicalPosition = transform.position;

        // Try to find the player immediately on start
        if (target == null)
        {
            FindPlayer();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            // Retry finding player if lost
            FindPlayer();
            if (target == null) return;
        }

        Vector3 desiredPosition = target.position + offset;
        
        if (enableBounds && cam != null)
        {
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = cam.aspect * camHalfHeight;

            // Calculate clamp limits ensuring the camera view stays inside the bounds
            float minX = minPosition.x + camHalfWidth;
            float maxX = maxPosition.x - camHalfWidth;
            float minY = minPosition.y + camHalfHeight;
            float maxY = maxPosition.y - camHalfHeight;

            // Handle case where bounds are smaller than camera view (center it)
            if (minX > maxX)
            {
                float centerX = (minPosition.x + maxPosition.x) / 2f;
                minX = centerX;
                maxX = centerX;
            }
            if (minY > maxY)
            {
                float centerY = (minPosition.y + maxPosition.y) / 2f;
                minY = centerY;
                maxY = centerY;
            }

            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
            desiredPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        }

        // Smoothly interpolate towards the desired position (Logical Position)
        logicalPosition = Vector3.Lerp(logicalPosition, desiredPosition, smoothSpeed);
        
        // Apply Shake on top of Logical Position
        Vector3 finalPosition = logicalPosition;

        if (shakeTimer > 0)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            finalPosition += shakeOffset;
            shakeTimer -= Time.unscaledDeltaTime;
        }

        transform.position = finalPosition;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    void FindPlayer()
    {
        // Find by type is generally fast enough for initialization
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            // Fallback for older Unity versions or if Player script is missing but tag is there
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    void OnDrawGizmos()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.green;
            
            // Calculate center and size for the gizmo
            float width = maxPosition.x - minPosition.x;
            float height = maxPosition.y - minPosition.y;
            Vector3 center = new Vector3(minPosition.x + width / 2, minPosition.y + height / 2, 0);
            Vector3 size = new Vector3(width, height, 0);

            Gizmos.DrawWireCube(center, size);
        }
    }
}
