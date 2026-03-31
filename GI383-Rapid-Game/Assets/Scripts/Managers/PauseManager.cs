using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Panel that contains the Resume and Exit buttons")]
    public GameObject settingsPanel;

    private bool isPaused = false;

    // Optional: If you want to use the 'Escape' key to toggle pause
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Toggles the pause state. Connect this to your 'Stop Button'.
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Pause();
        }
        else
        {
            ResumeGame();
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Resumes the game. Connect this to your 'Resume Button'.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Exits to Main Menu. Connect this to your 'Exit Button'.
    /// </summary>
    public void ExitGame()
    {
        // Ensure time is normal before leaving
        Time.timeScale = 1f;
        
        // Load Main Menu - Make sure to use the exact scene name
        SceneManager.LoadScene("MainMenu");
    }
}
