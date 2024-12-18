// Manages all UI elements including menus, score display, and lives display
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Singleton instance for global access


    [Header("Game Stats Menu")]
    [SerializeField] private GameObject gameStatsMenu;             // The menu panel
    [SerializeField] private TextMeshProUGUI statsLevelHighscore;  // Level best score
    [SerializeField] private TextMeshProUGUI statsLevelTime;       // Level best time
    [SerializeField] private TextMeshProUGUI statsOverallTime;     // Total time across levels
    [SerializeField] private TextMeshProUGUI statsOverallHighscore; // Total highscore

    // Skin system UI elements
    [SerializeField] private List<Button> skinButtons;          // Clickable skin selection buttons
    [SerializeField] private List<TextMeshProUGUI> equippedLabels;  // "Equipped" indicators
    [SerializeField] public List<int> unlockScores;           // Score requirements for each skin
    [SerializeField] private List<GameObject> buttonOverlays;   // Lock overlays for locked skins

    [Header("Gameplay UI Elements")]
    [SerializeField] private TextMeshProUGUI livesText;           // Lives display
    [SerializeField] private TextMeshProUGUI potentialScoreText;  // Displays potential score
    [SerializeField] private TextMeshProUGUI timerText;          // Timer display
    [SerializeField] private TextMeshProUGUI scoreText;          // Score display

    [Header("Game Over UI Elements")]
    [SerializeField] private GameObject gameOverMenuUI;           // Game over menu
    [SerializeField] private TextMeshProUGUI gameOverScoreUI;     // Final score display
    [SerializeField] private TextMeshProUGUI gameOverHighscoreUI; // Best score display
    [SerializeField] private TextMeshProUGUI overallTimeUI;       // Overall time display
    [SerializeField] private TextMeshProUGUI overallHighscoreUI;  // Overall highscore display
    [SerializeField] private TextMeshProUGUI gameOverTimeUI;      // Add this for best time

    [Header("Start Menu UI Elements")]
    [SerializeField] private GameObject startMenuUI;              // Main menu UI

    [Header("Pause Menu")]
    [SerializeField] private PausedMenu pausedMenu;               // Reference to pause menu

    private GameManager gm;                                       // Reference to GameManager

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPausedMenu(PausedMenu newPausedMenu)
    {
        pausedMenu = newPausedMenu;

        if (pausedMenu == null)
        {
            Debug.LogError("PausedMenu reference could not be set in UIManager!");
        }
    }


    // Updates skin button interactivity based on player's overall highscore
    public void UpdateSkinButtons()
    {
        if (skinButtons == null || unlockScores == null)
            return;

        for (int i = 0; i < skinButtons.Count; i++)
        {
            if (skinButtons[i] != null)
            {
                bool isUnlocked = gm != null && gm.data.overallHighscore >= unlockScores[i];
                skinButtons[i].interactable = isUnlocked;

                // Skip overlay for first skin (index 0), handle overlays for rest
                if (i > 0 && i - 1 < buttonOverlays.Count)  // i-1 because overlays list is offset by 1
                {
                    if (buttonOverlays[i - 1] != null)
                    {
                        buttonOverlays[i - 1].SetActive(!isUnlocked);
                    }
                }
            }
        }
    }

    // Handles skin selection
    public void OnSkinButtonClick(int skinIndex)
    {
        if (gm != null)
        {
            gm.EquipSkin(skinIndex);
            UpdateSkinEquippedLabels();
        }
    }

    // Updates "Equipped" label visibility
    public void UpdateSkinEquippedLabels()
    {
        for (int i = 0; i < equippedLabels.Count; i++)
        {
            if (equippedLabels[i] != null)
            {
                equippedLabels[i].gameObject.SetActive(i == gm.data.equippedSkinIndex);
            }
        }
    }

    private void Start()
    {
        // Get GameManager instance
        gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("GameManager instance is null in UIManager.");
            return;
        }

        // Update UI elements for new levels
        UpdateLivesUI(gm.lives);
        UpdatePotentialScoreUI(0); // Reset to 0 initially

        // Ensure GameOver UI is hidden
        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(false);

        // Subscribe to GameManager's GameOver event
        gm.onGameOver.AddListener(ActivateGameOverUI);

        // Initialize skin UI
        UpdateSkinButtons();
        UpdateSkinEquippedLabels();
    }

    private void Update()
    {
        if (gm != null && gm.isPlaying)
        {
            // Update potential score dynamically
            int potentialScore = gm.GetCurrentPotentialScore();
            potentialScoreText.text = $"Potential Score: {potentialScore}";

            // Update timer and score
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(gm.GetTimer())}";
            }
            if (scoreText != null)
            {
                scoreText.text = $"Score: {Mathf.FloorToInt(gm.currentScore)}";
            }
        }

        // Block ESC functionality until isPlaying is true
        if (gm == null || !gm.isPlaying)
            return;

        // ESC key functionality (now active after isPlaying is true)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC is now available.");
        }

        // These should be outside the isPlaying check
        if (gm != null)
        {
            UpdateSkinButtons();
        }

        // Pause toggle logic (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape) && pausedMenu != null)
        {
            if (pausedMenu.gameObject.activeSelf)
                pausedMenu.ResumeGame();
            else
                pausedMenu.PauseGame();
        }
    }

    public void UpdateLivesUI(int currentLives)
    {
        // Updates the lives display
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
        else
        {
            Debug.LogError("LivesText is not assigned in the UIManager!");
        }
    }

    public void ActivateGameOverUI()
    {
        StartCoroutine(ShowGameOverMenuAfterDelay());
    }

    public void RefreshGameStatsMenu()
    {
        if (GameManager.Instance == null || GameManager.Instance.data == null)
        {
            Debug.LogError("GameManager or saved data is null! Cannot refresh game stats.");
            return;
        }

        var data = GameManager.Instance.data;
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        int levelsCompleted = 0;

        foreach (float time in data.levelCompletionTimes)
        {
            if (time > 0) levelsCompleted++;
        }

        // Only show level-specific stats if this level was previously completed
        if (data.levelCompletionTimes[currentLevel] > 0)
        {
            if (statsLevelHighscore != null)
            {
                statsLevelHighscore.gameObject.SetActive(true);
                statsLevelHighscore.text = $"Level Highscore: {Mathf.FloorToInt(data.levelHighscores[currentLevel])}";

            }
            if (statsLevelTime != null)
            {
                statsLevelTime.gameObject.SetActive(true);
                statsLevelTime.text = $"Best Time: {Mathf.FloorToInt(data.levelCompletionTimes[currentLevel])} seconds";
            }
        }
        else
        {
            if (statsLevelHighscore != null)
                statsLevelHighscore.gameObject.SetActive(false);
            if (statsLevelTime != null)
                statsLevelTime.gameObject.SetActive(false);
        }

        // Always show overall stats if any level was completed
        if (levelsCompleted > 0)
        {
            if (statsOverallTime != null)
            {
                statsOverallTime.gameObject.SetActive(true);
                statsOverallTime.text = $"Total Time: {Mathf.FloorToInt(data.totalCompletionTime)} seconds (Level{levelsCompleted}/10)";
            }
            if (statsOverallHighscore != null)
            {
                statsOverallHighscore.gameObject.SetActive(true);
                statsOverallHighscore.text = $"Overall Highscore: {Mathf.FloorToInt(data.overallHighscore)} (Level{levelsCompleted}/10)";
            }
        }
        else
        {
            if (statsOverallTime != null)
                statsOverallTime.gameObject.SetActive(false);
            if (statsOverallHighscore != null)
                statsOverallHighscore.gameObject.SetActive(false);
        }
    }
    public void RetryLevel()
    {

        // Get fresh reference to GameManager
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // First disable the game over menu
        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(false);

        // Reset time scale before anything else
        Time.timeScale = 1f;

        // Reset game state
        gm.ResetLives();
        gm.isPlaying = false; // Temporarily set to false during reset

        // Find and reset player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (player != null && spawnPoint != null)
        {
            // Reset position
            player.transform.position = spawnPoint.transform.position;

            // Reset physics
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Reset ResetObject component
            ResetObject resetObject = player.GetComponent<ResetObject>();
            if (resetObject != null)
            {
                resetObject.ResetFallHandling();
                resetObject.SetBoundaryForCurrentLevel();
            }
        }
        else
        {
            Debug.LogError("Player or SpawnPoint not found in scene!");
            return;
        }

        // Update UI elements
        UpdateLivesUI(gm.lives);
        UpdatePotentialScoreUI(0);

        // Wait a frame before starting the game to ensure everything is properly reset
        StartCoroutine(StartGameNextFrame(gm));
    }

    private IEnumerator StartGameNextFrame(GameManager gm)
    {
        yield return null;
        gm.StartGame();
    }

    private IEnumerator ShowGameOverMenuAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        float levelTime = gm.data.levelCompletionTimes[currentLevel];
        float totalTime = gm.data.totalCompletionTime;
        int levelsCompleted = 0;

        foreach (float time in gm.data.levelCompletionTimes)
        {
            if (time > 0) levelsCompleted++;
        }

        // Activate Game Over UI and display stats
        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(true);

        // Always show current score
        if (gameOverScoreUI != null)
            gameOverScoreUI.text = $"Score: {Mathf.FloorToInt(gm.currentScore)}";

        // Only show level-specific stats if this level was previously completed
        if (gm.data.levelCompletionTimes[currentLevel] > 0)
        {
            if (gameOverHighscoreUI != null)
            {
                gameOverHighscoreUI.gameObject.SetActive(true);
                gameOverHighscoreUI.text = $"Level Highscore: {Mathf.FloorToInt(gm.data.levelHighscores[currentLevel])}";
            }
            if (gameOverTimeUI != null)
            {
                gameOverTimeUI.gameObject.SetActive(true);
                gameOverTimeUI.text = $"Best Time: {Mathf.FloorToInt(gm.data.levelCompletionTimes[currentLevel])} seconds";
            }
        }
        else
        {
            if (gameOverHighscoreUI != null)
                gameOverHighscoreUI.gameObject.SetActive(false);
            if (gameOverTimeUI != null)
                gameOverTimeUI.gameObject.SetActive(false);
        }

        // Always show overall stats if any level was completed
        if (levelsCompleted > 0)
        {
            if (overallTimeUI != null)
            {
                overallTimeUI.gameObject.SetActive(true);
                overallTimeUI.text = $"Total Time: {Mathf.FloorToInt(totalTime)} seconds ({levelsCompleted}/10 levels)";
            }
            if (overallHighscoreUI != null)
            {
                overallHighscoreUI.gameObject.SetActive(true);
                overallHighscoreUI.text = $"Overall Highscore: {Mathf.FloorToInt(gm.data.overallHighscore)}";
            }
        }
        else
        {
            if (overallTimeUI != null)
                overallTimeUI.gameObject.SetActive(false);
            if (overallHighscoreUI != null)
                overallHighscoreUI.gameObject.SetActive(false);
        }

        // Freeze the game after UI is shown
        yield return new WaitForSeconds(0.1f);  // Small additional delay to ensure UI is visible
        Time.timeScale = 0f;
    }

    public void PlayButtonHandler()
    {
        if (gm != null)
        {
            gm.StartGame();
        }

        if (pausedMenu != null)
        {
            pausedMenu.StartGame();
        }
    }

    public void ShowStartMenu()
    {
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(true);
        }
    }

    public void HideStartMenu()
    {
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(false);
        }
    }

    public void UpdatePotentialScoreUI(int potentialScore)
    {
        if (potentialScoreText != null)
        {
            potentialScoreText.text = $"Potential Score: {potentialScore}";
        }
    }
}