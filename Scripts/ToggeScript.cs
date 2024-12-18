using UnityEngine;

public class PauseMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject startMenuCanvas; // Reference to start menu
    private bool isPaused = false;

    private void Update()
    {
        // Don't allow pausing if start menu is active
        if (startMenuCanvas != null && startMenuCanvas.activeSelf)
        {
            if (isPaused)
            {
                ResumeGame(); // Make sure pause menu is closed if start menu becomes active
            }
            return;
        }

        // Only allow pausing if game is playing
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}