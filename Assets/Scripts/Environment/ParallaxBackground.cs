using UnityEngine;

[DefaultExecutionOrder(10)] // Ensures this runs AFTER CameraFollow (Default 0) in LateUpdate
public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 parallaxEffectMultiplier = new Vector2(0.5f, 0.5f); // (0,0) = static, (1,1) = moves with cam
    public bool infiniteHorizontal = true;
    public bool infiniteVertical = false;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            lastCameraPosition = cameraTransform.position;
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Texture2D texture = sprite.sprite.texture;
            textureUnitSizeX = texture.width / sprite.sprite.pixelsPerUnit;
            textureUnitSizeY = texture.height / sprite.sprite.pixelsPerUnit;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // Move the layer by a fraction of the camera movement
        transform.position += new Vector3(
            deltaMovement.x * parallaxEffectMultiplier.x, 
            deltaMovement.y * parallaxEffectMultiplier.y, 
            0);
            
        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            float distCheck = Mathf.Abs(cameraTransform.position.x - transform.position.x);
            
            if (distCheck >= textureUnitSizeX)
            {
                float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
            }
        }
        
        if (infiniteVertical && textureUnitSizeY > 0)
        {
            float distCheck = Mathf.Abs(cameraTransform.position.y - transform.position.y);

            if (distCheck >= textureUnitSizeY)
            {
                float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitSizeY;
                transform.position = new Vector3(transform.position.x, cameraTransform.position.y + offsetPositionY);
            }
        }
    }
}
