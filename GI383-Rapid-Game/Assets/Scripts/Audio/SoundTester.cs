using UnityEngine;

public class SoundTester : MonoBehaviour
{
    public string soundName = "TestSound";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"SoundTester: Playing '{soundName}' at {mousePos}");
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(soundName, mousePos);
            }
            else
            {
                Debug.LogWarning("SoundManager Instance is null!");
            }
        }
    }
}
