using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Reload the main menu (scene at build index 0)
    public void ReloadGame()
    {
        Debug.Log("Reloading main menu...");

        // Reset time scale in case the game is paused
        Time.timeScale = 1f;

        // Destroy PlayerManager if it exists
        GameObject playerManager = GameObject.FindObjectOfType<PlayerManager>()?.gameObject;
        if (playerManager != null)
        {
            Debug.Log("Destroying PlayerManager...");
            Destroy(playerManager);
        }

        // Destroy GameManager if it exists
        if (GameManager.Instance != null)
        {
            Debug.Log("Destroying GameManager...");
            Destroy(GameManager.Instance.gameObject);
            GameManager.Instance = null;
        }

        // Reload the main menu scene (build index 0)
        SceneManager.LoadScene(0);
    }

    // Load a specific level by build index
    public void LoadLevel(int levelIndex)
    {
        Debug.Log($"Loading level {levelIndex}...");
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(levelIndex);
    }

    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
