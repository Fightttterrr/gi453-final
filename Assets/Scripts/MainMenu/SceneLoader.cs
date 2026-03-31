using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName = "Worldtestของพรี่ชาย😇Scene";
    [SerializeField] private float freezeDuration = 0.5f;
    [SerializeField] private float glitchDuration = 1.0f;
    [SerializeField] private float loadingDelay = 2.0f;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI attemptText;

    [Header("Glitch Appearance")]
    [SerializeField] private int glitchBarCount = 50;
    [SerializeField] private Color[] glitchColors = new Color[] 
    { 
        Color.black, 
        Color.black, 
        Color.black, // Weighted towards black
        new Color(1f, 0f, 0.4f), // Hot Pink
        new Color(0.6f, 0f, 1f), // Purple
        Color.red 
    };

    private void Start()
    {
        if (attemptText != null)
        {
            // Prepare the text but hide it initially
            int attempts = PlayerPrefs.GetInt("AttemptCount", 0);
            attemptText.text = $"Attempt : {attempts}";
            attemptText.gameObject.SetActive(false);
        }
    }

    public void LoadGameScene()
    {
        // Increment Attempt Count
        int attempts = PlayerPrefs.GetInt("AttemptCount", 0) + 1;
        PlayerPrefs.SetInt("AttemptCount", attempts);

        if (attemptText != null)
        {
             attemptText.text = $"Attempt : {attempts}";
             // We don't activate it here anymore, we pass it to the controller to clone and show
        }

        // Create a standalone GameObject to handle the transition logic so it survives the scene change
        GameObject controllerObj = new GameObject("SceneTransitionController");
        TransitionController controller = controllerObj.AddComponent<TransitionController>();
        DontDestroyOnLoad(controllerObj);

        controller.StartTransition(targetSceneName, freezeDuration, glitchDuration, loadingDelay, fadeDuration, glitchBarCount, glitchColors, attemptText);
    }
}

// Separate component to handle the persistent transition logic
public class TransitionController : MonoBehaviour
{
    private Canvas transitionCanvas;
    private Image blackScreen;
    private List<Image> glitchBars = new List<Image>();

    public void StartTransition(string sceneName, float freezeDur, float glitchDur, float loadDelay, float fadeDur, int barCount, Color[] colors, TextMeshProUGUI overlayTextOriginal)
    {
        StartCoroutine(TransitionRoutine(sceneName, freezeDur, glitchDur, loadDelay, fadeDur, barCount, colors, overlayTextOriginal));
    }

    private IEnumerator TransitionRoutine(string targetSceneName, float freezeDuration, float glitchDuration, float loadingDelay, float fadeDuration, int barCount, Color[] colors, TextMeshProUGUI overlayTextOriginal)
    {
        // 1. Setup UI
        CreateTransitionUI(barCount);

        // 1.1 Clone Text if provided
        if (overlayTextOriginal != null)
        {
            // Activate original so the clone is active
            overlayTextOriginal.gameObject.SetActive(true);
            
            // Instantiate copy on the transition canvas to ensure it is on top
            TextMeshProUGUI textClone = Instantiate(overlayTextOriginal, transitionCanvas.transform);
            
            // Optional: reset scale/position if needed, but Instantiate usually keeps relative transform if parent matches. 
            // If parent was different, we might need to reset. 
            // Assuming original text is well positioned in screen space, we just want it to anchor similarly.
            // Let's ensure it's centered or preserves the rect.
            RectTransform cloneRect = textClone.rectTransform;
            cloneRect.anchoredPosition = overlayTextOriginal.rectTransform.anchoredPosition;
            cloneRect.sizeDelta = overlayTextOriginal.rectTransform.sizeDelta;
            cloneRect.anchorMin = overlayTextOriginal.rectTransform.anchorMin;
            cloneRect.anchorMax = overlayTextOriginal.rectTransform.anchorMax;

            // Hide original to avoid duplicates (optional, scene is changing anyway)
            overlayTextOriginal.gameObject.SetActive(false);
        }
        // Canvas is child of this controller, so it effectively persists with it, 
        // but we'll parent this controller to nothing to be sure.
        transform.SetParent(null);

        // 1.5. Freeze Phase
        if (freezeDuration > 0)
        {
             Time.timeScale = 0f;
             
             // Show a static glitch state
             if (glitchBars.Count > 0)
             {
                 // Activate random bars once for the freeze effect
                 foreach (var bar in glitchBars)
                 {
                     if (Random.value > 0.5f)
                     {
                         bar.gameObject.SetActive(true);
                         RectTransform rect = bar.rectTransform;
                         float yPos = Random.value;
                         float height = Random.Range(0.01f, 0.15f);
                         float xPos = Random.value > 0.8f ? Random.value : 0f;
                         float width = xPos > 0 ? Random.Range(0.1f, 0.5f) : 1f;

                         rect.anchorMin = new Vector2(xPos, yPos);
                         rect.anchorMax = new Vector2(xPos + width, yPos + height);
                         bar.color = colors[Random.Range(0, colors.Length)];
                     }
                 }
                 // Set background occasionally black or colored
                 blackScreen.color = new Color(0, 0, 0, 0.2f); 
             }

             yield return new WaitForSecondsRealtime(freezeDuration);
             
             Time.timeScale = 1f;
        }

        // 2. Glitch Effect
        float elapsed = 0f;
        while (elapsed < glitchDuration)
        {
            // Flash background color (black or dark red/purple occasionally)
            if (Random.value > 0.9f)
                blackScreen.color = colors[Random.Range(0, colors.Length)];
            else
                blackScreen.color = new Color(0, 0, 0, Random.Range(0f, 0.2f));

            // Scramble glitch bars
            foreach (var bar in glitchBars)
            {
                bool active = Random.value > 0.3f; // More active bars
                bar.gameObject.SetActive(active);
                if (active)
                {
                    RectTransform rect = bar.rectTransform;
                    
                    // Random blocky positioning
                    // Allow both horizontal strips and smaller "blocks"
                    float yPos = Random.value;
                    float height = Random.Range(0.01f, 0.15f); // 1% to 15% screen height
                    float xPos = Random.value > 0.8f ? Random.value : 0f; // Mostly full width, sometimes random x
                    float width = xPos > 0 ? Random.Range(0.1f, 0.5f) : 1f;

                    rect.anchorMin = new Vector2(xPos, yPos);
                    rect.anchorMax = new Vector2(xPos + width, yPos + height);
                    
                    // Pick a random color
                    bar.color = colors[Random.Range(0, colors.Length)];
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Solid Black Helper (Loading Start)
        blackScreen.color = Color.black;
        foreach (var bar in glitchBars) bar.gameObject.SetActive(false);

        yield return new WaitForSeconds(loadingDelay);

        // 4. Load Scene Async
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            // Wait until loaded
            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }
        else
        {
            Debug.LogError($"Scene '{targetSceneName}' could not be loaded. Check build settings.");
        }

        // Wait a frame for new scene initialization
        yield return null;

        // 5. Fade Out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Cleanup
        Destroy(gameObject);
    }

    private void CreateTransitionUI(int barCount)
    {
        // Create Canvas as a child of this controller
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform, false);
        
        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 9999; // On top of everything
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Fullscreen Black Image
        GameObject bgObj = new GameObject("BlackScreen");
        bgObj.transform.SetParent(canvasObj.transform, false);
        blackScreen = bgObj.AddComponent<Image>();
        blackScreen.color = Color.clear;
        RectTransform bgRect = blackScreen.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create Glitch Bars Pool
        for (int i = 0; i < barCount; i++)
        {
            GameObject barObj = new GameObject($"GlitchBar_{i}");
            barObj.transform.SetParent(canvasObj.transform, false);
            Image barImg = barObj.AddComponent<Image>();
            barImg.color = Color.black;
            barObj.SetActive(false);
            
            RectTransform rect = barImg.rectTransform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            glitchBars.Add(barImg);
        }
    }
}
