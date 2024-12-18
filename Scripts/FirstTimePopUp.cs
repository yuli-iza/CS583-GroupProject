// Script handles the welcome popup for first-time players, showing controls and basic tutorial
using UnityEngine;

public class FirstTimePopup : MonoBehaviour
{
    [SerializeField] private GameObject firstTimePopupCanvas;    // Reference to the popup UI canvas

    private bool isPopupActive = false; // Track popup state to ensure freeze

    // Called by Play button's OnClick event to check if player needs tutorial
    public void ShowPopupIfFirstTime()
    {
        Debug.Log("Play button clicked. Checking for first-time play...");

        if (IsFirstTimePlayer())
        {
            Debug.Log("First-time player detected. Showing popup.");
            ShowFirstTimePopup();
        }
        else
        {
            Debug.Log("Returning player. Starting game normally.");
            StartGame();
        }
    }

    // Checks save data to determine if this is a new player
    private bool IsFirstTimePlayer()
    {
        // Try to load existing save file
        string loadedData = SaveSystem.Load("save");
        if (loadedData == null)
        {
            Debug.Log("No save data found. This is a first-time player.");
            return true; // No save file exists, must be first time
        }

        // Deserialize save data and check the isFirstTimeShown flag
        SaveData data = JsonUtility.FromJson<SaveData>(loadedData);
        bool isFirstTime = !data.isFirstTimeShown; // True if flag is false
        Debug.Log("Checking save data. IsFirstTimePlayer: " + isFirstTime);
        return isFirstTime;
    }


    // Shows welcome message and freezes game time
    private void ShowFirstTimePopup()
    {
        Debug.Log("Displaying first-time popup and freezing game.");
        Time.timeScale = 0f;                  // Pause game time while showing tutorial
        firstTimePopupCanvas.SetActive(true); // Display the tutorial popup
        isPopupActive = true;                 // Track popup activation
    }

    // Called when player clicks to close the tutorial
    public void DismissFirstTimePopup()
    {
        Debug.Log("Popup dismissed. Resuming game.");
        firstTimePopupCanvas.SetActive(false); // Hide the popup
        Time.timeScale = 1f;                   // Resume normal game time
        isPopupActive = false;                 // Track popup deactivation

        // Update the save data
        SaveData data = new SaveData();
        string loadedData = SaveSystem.Load("save");
        if (loadedData != null)
        {
            data = JsonUtility.FromJson<SaveData>(loadedData);
        }

        data.isFirstTimeShown = true; // Set the flag to true
        string saveString = JsonUtility.ToJson(data);
        SaveSystem.Save("save", saveString); // Save updated progress
        Debug.Log("isFirstTimeShown flag set to true and saved.");

        StartGame(); // Start gameplay
    }


    private void Update()
    {
        // Enforce time freeze while popup is active
        if (isPopupActive && Time.timeScale != 0f)
        {
            Debug.Log("Reapplying freeze as Time.timeScale was changed.");
            Time.timeScale = 0f;
        }
    }

    // Begins the actual game after handling first-time setup
    private void StartGame()
    {
        Debug.Log("Game is starting...");
        // Add game start logic here
    }
}
