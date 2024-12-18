using UnityEngine;
using UnityEngine.SceneManagement;

public class PausedMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject startMenuUI; // Reference to StartMenu on different canvas
    private bool isPaused = false;
    private static PausedMenu instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    private void Update()
    {
        // First check if StartMenu is active
        if (startMenuUI != null && startMenuUI.activeSelf)
        {
            // If pause menu is open while start menu becomes active, close it
            if (isPaused)
            {
                ResumeGame();
            }
            return; // Don't process ESC key if start menu is active
        }

        // Then check if game is playing
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
        {
            return;
        }

        // Handle ESC key only if start menu is inactive and game is playing
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

    public void StartGame()
    {
        ResumeGame();
        Debug.Log("Game started.");
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}