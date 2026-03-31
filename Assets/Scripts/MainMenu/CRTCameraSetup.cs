using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CRTCameraSetup : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The camera to render to the texture. Defaults to Camera.main if empty.")]
    public Camera targetCamera;
    
    [Tooltip("Resolution scaling. 1.0 = Screen Resolution.")]
    [Range(0.1f, 1.0f)]
    public float resolutionScale = 1.0f;

    [Tooltip("Create a specific Material with the URP CRT shader, or assign one.")]
    public Material crtMaterial;

    private RenderTexture renderTexture;
    private RawImage rawImage;
    private Camera outputCamera;

    void Start()
    {
        InitializeCRT();
    }

    void InitializeCRT()
    {
        // 1. Find Camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("CRTCameraSetup: No Main Camera found!");
                return;
            }
        }

        // 2. Create Render Texture (ALWAYS FULL RESOLUTION for UI Clicks)
        int width = Screen.width;
        int height = Screen.height;
        
        // Ensure valid dimensions
        width = Mathf.Max(width, 256);
        height = Mathf.Max(height, 256);

        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = "CRT_RenderTexture";
        renderTexture.antiAliasing = 1; // Optional
        renderTexture.filterMode = FilterMode.Point; // Crisp pixels

        // 3. Assign to Camera
        targetCamera.targetTexture = renderTexture;

        // 4. Assign to RawImage
        rawImage = GetComponent<RawImage>();
        rawImage.texture = renderTexture;
        rawImage.raycastTarget = false; // Fix: Ensure CRT overlay doesn't block UI clicks
        
        // 5. Ensure Material is correct and Set Resolution
        if (rawImage.material == null || rawImage.material.shader.name != "Custom/URP_CRT_Distortion")
        {
             if (crtMaterial != null)
             {
                 rawImage.material = crtMaterial;
             }
        }

        // Apply Pixelation via Shader
        if (rawImage.material != null)
        {
            // Calculate the "Fake" resolution based on the slider
            float targetW = Screen.width * resolutionScale;
            float targetH = Screen.height * resolutionScale;
            
            rawImage.material.SetVector("_Resolution", new Vector4(targetW, targetH, 0, 0));
        }

        // 6. Create Output Camera (The Fix)
        CreateOutputCamera();
    }

    void CreateOutputCamera()
    {
        // Check if we already have one
        GameObject camObj = GameObject.Find("CRT_Output_Camera");
        if (camObj != null)
        {
            outputCamera = camObj.GetComponent<Camera>();
        }
        else
        {
            camObj = new GameObject("CRT_Output_Camera");
            outputCamera = camObj.AddComponent<Camera>();
        }

        // Configure Output Camera
        // It should render "Nothing" efficiently, or just UI if UI is Screen Space - Camera
        // Since we likely have Screen Space - Overlay for UI, this camera just needs to exist
        // to tell Unity "We are rendering to the screen".
        
        outputCamera.clearFlags = CameraClearFlags.SolidColor;
        outputCamera.backgroundColor = Color.black; 
        outputCamera.cullingMask = 0; // Render nothing from the scene geometry
        outputCamera.depth = 100; // Render AFTER the main camera
        outputCamera.allowHDR = false;
        outputCamera.allowMSAA = false;
        
        // This camera outputs to the screen (null = screen)
        outputCamera.targetTexture = null; 
    }

    void OnDestroy()
    {
        // Cleanup target camera
        if (targetCamera != null && targetCamera.targetTexture == renderTexture)
        {
            targetCamera.targetTexture = null;
        }

        // Cleanup RT
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        // Cleanup Output Camera (Automatic)
        if (outputCamera != null)
        {
            Destroy(outputCamera.gameObject);
        }
    }
}
