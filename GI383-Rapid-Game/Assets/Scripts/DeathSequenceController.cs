using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathSequenceController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The panel containing the restart button")]
    public GameObject deathPanel; 
    
    [Tooltip("Image used for full screen flickering effect")]
    public Image flickerImage;    
    
    [Tooltip("The Video Player component for Game Over video")]
    public VideoPlayer gameOverVideo;
    
    [Tooltip("The RawImage component to display the video on")]
    public RawImage videoDisplayImage; 

    [Header("CRT Input Settings")]
    [Tooltip("The RawImage that displays the final CRT output (User Click Area).")]
    public RawImage crtScreenRawImage;
    [Tooltip("The Camera that renders the UI to the RenderTexture.")]
    public Camera crtOutputCamera; 

    [Header("Settings")]
    public float flickerDuration = 0.5f;
    public float flickerSpeed = 0.05f;
    public float delayBeforeButton = 10.0f; // Time to wait after video starts before showing restart button

    public void StartDeathSequence()
    {
        Debug.Log("DeathSequenceController: Starting death sequence...");
        this.gameObject.SetActive(true);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Ensure time is running for the sequence (or use Realtime)
        Time.timeScale = 0f; // Stop the game action immediately

        // 1. Desperate Flickering (Red/Black)
        if (flickerImage != null)
        {
            flickerImage.gameObject.SetActive(true);
            float timer = 0;
            bool isRed = true;
            
            while(timer < flickerDuration)
            {
                flickerImage.color = isRed ? new Color(0.8f, 0f, 0f, 0.4f) : Color.black;
                isRed = !isRed;
                
                yield return new WaitForSecondsRealtime(flickerSpeed);
                timer += flickerSpeed;
            }
            
            flickerImage.color = Color.black; 
            flickerImage.gameObject.SetActive(false); 
        }

        // 2. Play Video (With Auto Render Texture Setup)
        if (videoDisplayImage != null && gameOverVideo != null)
        {
            videoDisplayImage.gameObject.SetActive(true);
            
            // Prepare Video
            gameOverVideo.Stop();
            gameOverVideo.renderMode = VideoRenderMode.RenderTexture;
            gameOverVideo.isLooping = false; // Ensure it doesn't loop

            // Create a temporary Render Texture if one doesn't exist or isn't assigned
            if (gameOverVideo.targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                renderTexture.name = "VideoRenderTexture";
                gameOverVideo.targetTexture = renderTexture;
            }

            // Assign the Render Texture to the Raw Image
            videoDisplayImage.texture = gameOverVideo.targetTexture;
            // Set Color to white to ensure texture is visible
            videoDisplayImage.color = Color.white;
            videoDisplayImage.raycastTarget = false; // Fix: Ensure Video overlay doesn't block Death Panel clicks

            gameOverVideo.Play();
            
            // Wait while video plays
            yield return new WaitForSecondsRealtime(delayBeforeButton);
        }
        /*else
        {
            // If no video, just wait a bit
            yield return new WaitForSecondsRealtime(1.0f);
        }*/

        // 3. Show Restart Button (Death Panel)
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);

            // Fix: Setup Input for Screen Space - Camera rendered to Texture
            // We need to swap the GraphicRaycaster for our custom RenderTextureRaycaster
            SetupRenderTextureRaycaster(deathPanel);
        }
    }

    private void SetupRenderTextureRaycaster(GameObject panel)
    {
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // 1. Disable default Graphic Raycaster (it fails on RTs)
        GraphicRaycaster defaultRaycaster = canvas.GetComponent<GraphicRaycaster>();
        if (defaultRaycaster != null && !(defaultRaycaster is Game.UI.RenderTextureRaycaster))
        {
            defaultRaycaster.enabled = false;
        }

        // 2. Add or Get our Custom Raycaster
        Game.UI.RenderTextureRaycaster customRaycaster = canvas.GetComponent<Game.UI.RenderTextureRaycaster>();
        if (customRaycaster == null)
        {
            customRaycaster = canvas.gameObject.AddComponent<Game.UI.RenderTextureRaycaster>();
        }

        // 3. Configure dependencies
        // Priority 1: Manual Assignment
        if (crtOutputCamera != null && crtScreenRawImage != null)
        {
            customRaycaster.renderTextureCamera = crtOutputCamera;
            customRaycaster.screenRawImage = crtScreenRawImage;
            return;
        }

        // Priority 2: Auto-Find CRTCameraSetup (Fallback)
        CRTCameraSetup crtSetup = FindFirstObjectByType<CRTCameraSetup>();
        if (crtSetup != null)
        {
             customRaycaster.renderTextureCamera = crtSetup.targetCamera;
             customRaycaster.screenRawImage = crtSetup.GetComponent<RawImage>();
        }
        else
        {
            Debug.LogWarning("DeathSequence: Could not find CRT Setup (Manual or Auto). Input might fail. Please assign 'Crt Screen Raw Image' and 'Crt Output Camera' in the Inspector.");
        }
    }

    public void RestartGame()
    {
        // Clean up render texture if we created one (optional, but good practice)
        if (gameOverVideo != null && gameOverVideo.targetTexture != null)
        {
            gameOverVideo.targetTexture.Release();
        }

        Time.timeScale = 1f; // Ensure time is running again
        SceneManager.LoadScene("MainMenu");
    }
}
