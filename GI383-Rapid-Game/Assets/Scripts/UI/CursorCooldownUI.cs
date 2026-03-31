using UnityEngine;
using UnityEngine.UI;

public class CursorCooldownUI : MonoBehaviour
{
    [Header("Configuration")]
    public float cursorSize = 50f;
    public Color cooldownColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Dependencies")]
    public PlayerCombat playerCombat;
    
    private Image cooldownImage;
    private Canvas canvas;
    private RectTransform rectTransform;

    void Awake()
    {
        // 1. Setup Canvas if needed
        SetupCanvas();
        
        // 2. Setup Image
        SetupImage();
        
        // 3. Find PlayerCombat if missing
        if (playerCombat == null)
        {
            playerCombat = FindObjectOfType<PlayerCombat>();
            // If still null, try finding via Player tag or verify script execution order
        }
    }

    void OnDisable()
    {
        // Ensure cursor is restored if this object is disabled/destroyed
        Cursor.visible = true;
    }

    void SetupCanvas()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CursorCooldownCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // High sorting order to be on top of everything
            canvas.sortingOrder = 999; 
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            transform.SetParent(canvasObj.transform, false);
        }
        
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(cursorSize, cursorSize);
    }
    
    void SetupImage()
    {
        cooldownImage = GetComponent<Image>();
        if (cooldownImage == null)
        {
            cooldownImage = gameObject.AddComponent<Image>();
        }
        
        if (cooldownImage.sprite == null)
        {
            cooldownImage.sprite = CreateCircleSprite();
        }
        
        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillClockwise = true;
        cooldownImage.fillOrigin = (int)Image.Origin360.Top;
        cooldownImage.color = cooldownColor;
        cooldownImage.raycastTarget = false;
    }
    
    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius && dist >= radius - 8) 
                {
                    colors[y * size + x] = Color.white;
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        // 1. Follow Mouse
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            transform.position = Input.mousePosition;
        }
        else
        {
            try
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, 
                    Input.mousePosition, 
                    canvas.worldCamera, 
                    out Vector2 pos);
                transform.position = canvas.transform.TransformPoint(pos);
            }
            catch
            {
                // Fallback
                transform.position = Input.mousePosition;
            }
        }
        
        // 2. Update Progress & Cursor Visibility
        UpdateCooldownAndCursor();
    }
    
    void UpdateCooldownAndCursor()
    {
        if (playerCombat == null) 
        {
            // If combat is missing, arguably we should show the default cursor
            if (!Cursor.visible) Cursor.visible = true;
            cooldownImage.enabled = false;
            return;
        }
        
        float progress = playerCombat.GetAttackCooldownProgress();
        
        if (progress > 0)
        {
            // COOLDOWN ACTIVE: Show Graphic, Hide System Cursor
            if (!cooldownImage.enabled) cooldownImage.enabled = true;
            cooldownImage.fillAmount = progress;
            
            if (Cursor.visible) Cursor.visible = false;
        }
        else
        {
            // COOLDOWN READY: Hide Graphic, Show System Cursor
            if (cooldownImage.enabled) cooldownImage.enabled = false;
            
            if (!Cursor.visible) Cursor.visible = true;
        }
    }
}
