using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using TMPro; // For TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool IsSoundOn()
    {
        return PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }

    public float currentScore = 0f;
    public SaveData data;
    public bool isPlaying = false;
    public UnityEvent onPlay = new UnityEvent();
    public UnityEvent onGameOver = new UnityEvent();


    [Header("Level Timer")]
    [SerializeField] public float[] levelDurations;  // Timer duration per level
    private float timer;
    private int currentLevelIndex;

    [Header("Character Customization")]
    [SerializeField] private List<GameObject> skinModels;

    [Header("Post-Processing")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxLensDistortion = 0.5f;
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private float lensDistortionThreshold = 50f;
    [SerializeField] private float vignetteThreshold = 100f;

    private LensDistortion lensDistortion;
    private Vignette vignette;

    
    [System.Serializable]
    public class BackgroundMusic
    {
        public string trackName;
        public AudioClip music;
        public bool isDefault;
    }
    [Header("Music System")]
    public BackgroundMusic[] backgroundTracks;
    [SerializeField] public AudioSource backgroundMusic;
    private int currentTrackIndex = 0;
    public TMP_Dropdown musicDropdown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (backgroundMusic != null)
            {
                backgroundMusic.playOnAwake = false;
                // Only play if sound is enabled in player preferences
                if (IsSoundOn())
                {
                    backgroundMusic.Play();
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        string loadedData = SaveSystem.Load("save");
        data = loadedData != null ? JsonUtility.FromJson<SaveData>(loadedData) : new SaveData();

        // Add scene loaded listener
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Equip the saved skin
        int skinToEquip = data.equippedSkinIndex >= 0 ? data.equippedSkinIndex : 0;
        EquipSkin(skinToEquip);

        // Initialize Post-Processing effects
        if (globalVolume != null && globalVolume.profile.TryGet(out lensDistortion) && globalVolume.profile.TryGet(out vignette))
        {
            lensDistortion.intensity.value = 0f;
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("Post-Processing volume or effects not assigned correctly.");
        }

        // Initialize level
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        timer = levelDurations[currentLevelIndex];

        // Initialize music system
        SetupBackgroundMusic();
        SetupMusicDropdown();
    }

    private void SetupBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            backgroundMusic = gameObject.AddComponent<AudioSource>();
        }

        backgroundMusic.loop = true;
        backgroundMusic.volume = 0.3f;
        backgroundMusic.playOnAwake = true;

        // Find and play default track
        for (int i = 0; i < backgroundTracks.Length; i++)
        {
            if (backgroundTracks[i].isDefault)
            {
                currentTrackIndex = i;
                break;
            }
        }

        if (backgroundTracks.Length > 0)
        {
            PlayBackgroundTrack(currentTrackIndex);
        }
    }

    private void SetupMusicDropdown()
    {
        if (musicDropdown != null)
        {
            musicDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (var track in backgroundTracks)
            {
                options.Add(new TMP_Dropdown.OptionData(track.trackName));
            }

            musicDropdown.AddOptions(options);
            musicDropdown.value = currentTrackIndex;
            musicDropdown.onValueChanged.AddListener(OnMusicSelectionChanged);
        }
    }

    public void OnMusicSelectionChanged(int index)
    {
        PlayBackgroundTrack(index);
    }

    private void PlayBackgroundTrack(int index)
    {
        if (index >= 0 && index < backgroundTracks.Length)
        {
            currentTrackIndex = index;
            backgroundMusic.clip = backgroundTracks[index].music;
            backgroundMusic.Play();
        }
    }

    private void Update()
    {
        if (isPlaying)
        {
            timer -= Time.deltaTime;

            if (timer <= 0.9f)
            {
                GameOver(); // Trigger Game Over immediately
                return;     // Skip the rest of Update()
            }

            UpdatePotentialScore();
            ApplyPostProcessingEffects();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        currentLevelIndex = scene.buildIndex;
        if (currentLevelIndex < levelDurations.Length)
        {
            timer = levelDurations[currentLevelIndex];
            isPlaying = false;
            UpdatePotentialScore();
            if (currentLevelIndex > 0)
            {
                StartGame();
            }

            // Find PausedManager and its PausedMenu script (including inactive objects)
            GameObject pausedManagerObject = FindInactiveObjectByName("PausedManager");

            if (pausedManagerObject != null)
            {
                PausedMenu pausedMenu = pausedManagerObject.GetComponent<PausedMenu>();

                if (pausedMenu != null && UIManager.Instance != null)
                {
                    UIManager.Instance.SetPausedMenu(pausedMenu);
                    Debug.Log("PausedMenu successfully set in UIManager.");
                }
                else
                {
                    Debug.LogError("PausedMenu script not found on PausedManager or UIManager is null!");
                }
            }
            else
            {
                Debug.LogError("PausedManager GameObject not found in scene!");
            }

            // Ensure background music continues
            if (backgroundMusic != null && !backgroundMusic.isPlaying)
            {
                backgroundMusic.Play();
            }
        }
        else
        {
            Debug.LogError($"Level duration not set for level {currentLevelIndex}!");
        }
    }

    // Custom method to find inactive GameObjects by name
    private GameObject FindInactiveObjectByName(string name)
    {
        Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.None && obj.name == name && obj.gameObject.scene.isLoaded)
            {
                return obj.gameObject;
            }
        }
        return null;
    }




    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public int GetCurrentPotentialScore()
    {
        return CalculateDynamicScore(timer, levelDurations[currentLevelIndex]);
    }

    private int CalculateDynamicScore(float remainingTime, float totalTime)
    {
        // Define quarter thresholds
        float firstQuarter = totalTime * 0.75f;
        float secondQuarter = totalTime * 0.5f;
        float thirdQuarter = totalTime * 0.25f;

        // Determine score threshold
        if (remainingTime >= firstQuarter)
        {
            // First quarter: Score decreases from 100 to 76
            return Mathf.FloorToInt(76 + (remainingTime - firstQuarter) / (totalTime * 0.25f) * 24);
        }
        else if (remainingTime >= secondQuarter)
        {
            // Second quarter: Score decreases from 75 to 51
            return Mathf.FloorToInt(51 + (remainingTime - secondQuarter) / (totalTime * 0.25f) * 24);
        }
        else if (remainingTime >= thirdQuarter)
        {
            // Third quarter: Score decreases from 50 to 26
            return Mathf.FloorToInt(26 + (remainingTime - thirdQuarter) / (totalTime * 0.25f) * 24);
        }
        else
        {
            // Fourth quarter: Score decreases from 25 to 1
            return Mathf.FloorToInt(1 + remainingTime / (totalTime * 0.25f) * 24);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f; // Make sure time is running when starting
        Debug.Log($"Starting game with level index: {currentLevelIndex}");
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        timer = levelDurations[currentLevelIndex]; // Reset timer for new level

        Debug.Log($"Setting timer to: {timer} for level {currentLevelIndex}"); // Add this debug log

        if (backgroundMusic != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();

        onPlay.Invoke();
        isPlaying = true;
        currentScore = 0;
        ResetLives();
    }

    public void GameOver()
    {
        isPlaying = false;

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        int finalScore = CalculateDynamicScore(timer, levelDurations[currentLevel]);
        currentScore = finalScore; // Set current score

        // DON'T update highscore here at all - only LevelComplete should do that
        // No need to check or update levelHighscores

        // Recalculate total overall highscore and total completion time
        data.overallHighscore = 0f;
        data.totalCompletionTime = 0f;

        foreach (float score in data.levelHighscores)
            data.overallHighscore += score;

        foreach (float time in data.levelCompletionTimes)
            data.totalCompletionTime += time;

        // Stop background music
        if (backgroundMusic != null && backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }

        SaveProgress();
        onGameOver.Invoke();
        // Automatically refresh stats in the Game Stats Menu
       


        Debug.Log($"Game Over! Final Score: {currentScore}");
    }

    public void LevelComplete()
    {
        isPlaying = false;

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        int finalScore = CalculateDynamicScore(timer, levelDurations[currentLevel]);
        currentScore = finalScore;

        // ALWAYS save completion time and highscore when level is completed
        float timeTaken = levelDurations[currentLevel] - timer;
        data.levelCompletionTimes[currentLevel] = timeTaken;

        // Update highscore if better
        if (data.levelHighscores[currentLevel] < currentScore)
        {
            data.levelHighscores[currentLevel] = currentScore;
        }

        // Calculate overall highscore
        data.overallHighscore = 0f;
        foreach (float score in data.levelHighscores)
            data.overallHighscore += score;

        // Calculate total completion time
        data.totalCompletionTime = 0f;
        foreach (float time in data.levelCompletionTimes)
            data.totalCompletionTime += time;

        SaveProgress();

        // Automatically refresh stats in the Game Stats Menu
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshGameStatsMenu();
        }

        Debug.Log($"Level Complete! Final Score: {currentScore}");
        currentScore = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ApplyPostProcessingEffects()
    {
        if (lensDistortion != null && currentScore >= lensDistortionThreshold)
        {
            float lensFactor = Mathf.Clamp01((currentScore - lensDistortionThreshold) / (vignetteThreshold - lensDistortionThreshold));
            lensDistortion.intensity.value = Mathf.Lerp(0f, maxLensDistortion, lensFactor);
        }

        if (vignette != null && currentScore >= vignetteThreshold)
        {
            float vignetteFactor = Mathf.Clamp01((currentScore - vignetteThreshold) / (vignetteThreshold * 0.5f));
            vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, vignetteFactor);
        }
    }

    public void EquipSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < skinModels.Count)
        {
            Debug.Log($"Attempting to equip skin {skinIndex}");

            // Use unlockScores from UIManager to validate unlock condition
            int requiredScore = UIManager.Instance.unlockScores[skinIndex];
            if (data.overallHighscore < requiredScore)
            {
                Debug.LogWarning($"Skin {skinIndex} is locked. Highscore of {requiredScore} required.");
                return;
            }

            // Deactivate all skins
            foreach (GameObject skin in skinModels)
            {
                skin.SetActive(false);
            }

            // Activate the selected skin
            skinModels[skinIndex].SetActive(true);
            Debug.Log($"Skin {skinModels[skinIndex].name} is now active.");

            // Save the equipped skin index
            data.equippedSkinIndex = skinIndex;
            SaveProgress();

            // Update UI elements to reflect the changes
            UIManager.Instance.UpdateSkinEquippedLabels();
            UIManager.Instance.UpdateSkinButtons();
        }
        else
        {
            Debug.LogError($"Invalid skin index: {skinIndex}. Check your skinModels list in GameManager.");
        }
    }



    public float GetTimer() => timer;

    public string PrettyScore() => Mathf.RoundToInt(currentScore).ToString();

    public string PrettyHighscore() => Mathf.RoundToInt(data.overallHighscore).ToString();



    public void SaveProgress()
    {
        string saveString = JsonUtility.ToJson(data);
        SaveSystem.Save("save", saveString);
    }

    // Clears all save data
    public void ResetGame()
    {
        SaveSystem.ClearSave("save");
        Debug.Log("Save data has been cleared successfully.");
    }

    public void UpdatePotentialScore()
    {
        int potentialScore = GetCurrentPotentialScore();

        // Update the UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdatePotentialScoreUI(potentialScore);
        }
    }

    public int lives = 3; // Central lives variable

    public void LoseLife()
    {
        if (lives > 1) // Only decrease lives if it's more than 1
        {
            lives--;

            // Update the Lives UI
            UIManager.Instance.UpdateLivesUI(lives);
        }
        else
        {
            // Directly trigger Game Over if lives == 1
            lives = 0; // Ensure lives show 0
            UIManager.Instance.UpdateLivesUI(lives);
            GameOver();
        }
    }

    public void ResetLives()
    {
        lives = 3;
        UIManager.Instance.UpdateLivesUI(lives);
    }
}