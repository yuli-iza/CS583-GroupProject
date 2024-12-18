using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Target : MonoBehaviour
{
    [Header("Game Finished UI")]
    [SerializeField] private GameObject gameFinishedUI; // Assign the Game Finished Canvas in the Inspector
    [SerializeField] private AudioClip correctSound; // Sound to play when reaching target
    private bool levelCompleted = false; // To prevent multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player reached the target
        if (!levelCompleted && other.CompareTag("Player"))
        {
            levelCompleted = true; // Prevent re-triggering
            Debug.Log("Target reached!");

            // Save initial progress if GameManager exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveProgress();
            }

            // Check if the current scene is "Level10"
            if (SceneManager.GetActiveScene().name == "Level10")
            {
                StartCoroutine(CompleteLevel10());
            }
            else
            {
                StartCoroutine(PlaySoundAndLoadNext());
            }
        }
    }

    private IEnumerator CompleteLevel10()
    {
        Debug.Log("Game Finished: Level 10 completed!");

        // Play completion sound first
        if (correctSound != null)
        {
            AudioSource.PlayClipAtPoint(correctSound, GameObject.Find("Main Camera").transform.position);
            yield return new WaitForSeconds(0.5f);
        }

        // Complete the level and calculate scores
        if (GameManager.Instance != null)
        {
            // Stop gameplay
            GameManager.Instance.isPlaying = false;
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            int finalScore = GameManager.Instance.GetCurrentPotentialScore();
            GameManager.Instance.currentScore = finalScore;

            // Save completion time
            float timeTaken = GameManager.Instance.levelDurations[currentLevel] - GameManager.Instance.GetTimer();
            GameManager.Instance.data.levelCompletionTimes[currentLevel] = timeTaken;

            // Update highscore if better
            if (GameManager.Instance.data.levelHighscores[currentLevel] < finalScore)
            {
                GameManager.Instance.data.levelHighscores[currentLevel] = finalScore;
            }

            // Calculate overall highscore
            GameManager.Instance.data.overallHighscore = 0f;
            foreach (float score in GameManager.Instance.data.levelHighscores)
            {
                GameManager.Instance.data.overallHighscore += score;
            }

            // Calculate total completion time
            GameManager.Instance.data.totalCompletionTime = 0f;
            foreach (float time in GameManager.Instance.data.levelCompletionTimes)
            {
                GameManager.Instance.data.totalCompletionTime += time;
            }

            // Save all progress
            GameManager.Instance.SaveProgress();

            // Update UI stats before showing completion screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RefreshGameStatsMenu();
            }
        }

        // Finally show the completion UI and freeze the game
        if (gameFinishedUI != null)
        {
            gameFinishedUI.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    private IEnumerator PlaySoundAndLoadNext()
    {
        if (correctSound != null)
        {
            // Play sound at Main Camera position
            AudioSource.PlayClipAtPoint(correctSound, GameObject.Find("Main Camera").transform.position);
            yield return new WaitForSeconds(0.3f);
        }

        // Notify GameManager to proceed with Level Complete logic
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}