using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;             

public class ResetObject : MonoBehaviour
{
    private float boundary;                       // Y-axis boundary, now dynamically set
    private int remainingLives = 3;              // Number of lives per level
    private Vector3 spawnPoint;                  // Original spawn point
    private GameManager gameManager;             // Reference to GameManager
    private bool isHandlingFall = false;         // Add this flag

    private void Start()
    {
        gameManager = GameManager.Instance;      // Get GameManager reference
        gameManager.ResetLives();               // Reset lives at the start
        spawnPoint = transform.position;         // Set spawn point
        SetBoundaryForCurrentLevel();            // Set initial boundary

        // Subscribe to scene loading to update boundary
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetBoundaryForCurrentLevel();            // Update boundary for new level
        isHandlingFall = false;                  // Reset the flag on new level
    }

    public void SetBoundaryForCurrentLevel()
    {
        string currentLevel = SceneManager.GetActiveScene().name;

        // Set boundary based on level name
        switch (currentLevel)
        {
            case "Level1":
                boundary = -3f;
                break;
            case "Level2":
                boundary = -20f;
                break;
            case "Level3":
                boundary = -20f;
                break;
            case "Level4":
                boundary = -40f;
                break;
            case "Level5":
                boundary = -145f;
                break;
            case "Level6":
                boundary = -45f;
                break;
            case "Level7":
                boundary = -10f;
                break;
            case "Level8":
                boundary = 35f;
                break;
            case "Level9":
                boundary = -45f;
                break;
            case "Level10":
                boundary = -30f;
                break;
            default:
                boundary = -20f;                 // Default fallback
                Debug.LogWarning($"No specific boundary set for {currentLevel}, using default.");
                break;
        }

        Debug.Log($"Boundary set to {boundary} for {currentLevel}");
    }

    private void Update()
    {
        if (transform.position.y < boundary && !isHandlingFall)  // Add flag check
        {
            HandleFall();
        }
    }

    private void HandleFall()
    {
        if (isHandlingFall) return;              // Extra safety check
        isHandlingFall = true;                   // Set the flag

        Debug.Log("Player fell! Losing a life.");
        gameManager.isPlaying = false;          // Pause the game/timer
        gameManager.LoseLife();

        if (gameManager.lives > 0)
        {
            StartCoroutine(RespawnSequence());
        }
        // GameOver() is already handled inside GameManager when lives <= 0
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(1f);    // Wait a second before respawning
        ResetPlayerPosition();
        yield return new WaitForSeconds(0.5f);  // Short pause before resuming
        gameManager.isPlaying = true;           // Resume the game/timer
        isHandlingFall = false;                 // Reset the flag
    }

    private void ResetPlayerPosition()
    {
        // Find current level's spawn point
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;       // Stop momentum
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log("Player respawned at current level's spawn point.");
        }
        else
        {
            Debug.LogError("No SpawnPoint found in current scene!");
        }
    }

    public void ResetLives()
    {
        remainingLives = 3;
        Debug.Log("Lives reset to 3.");
    }

   
    public void ResetFallHandling()
    {
        isHandlingFall = false;
    }
}