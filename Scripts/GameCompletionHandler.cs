using UnityEngine;
using TMPro;

public class GameCompletionHandler : MonoBehaviour
{
    [Header("Game Finished UI")]
    [SerializeField] private GameObject gameFinishedUI; // Reference to the Game Finished canvas
    [SerializeField] private TextMeshProUGUI gameFinishedText; // Optional: Text to display the message

    private bool gameFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the target area
        if (!gameFinished && other.CompareTag("Player"))
        {
            Debug.Log("Target reached! Game finished.");

            // Show the Game Finished UI
            if (gameFinishedUI != null)
            {
                gameFinishedUI.SetActive(true);
            }

            if (gameFinishedText != null)
            {
                gameFinishedText.text = "Congratulations! You Finished the Game!";
            }

            // Freeze the game
            Time.timeScale = 0f;

            // Mark the game as finished to prevent multiple triggers
            gameFinished = true;
        }
    }
}
