using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Target : MonoBehaviour
{
    // AudioSource to play a sound when the player reaches the target
    public AudioSource collisionSound;

    // Triggered when another collider enters this object's collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has a PlayerController component (i.e., it's the player)
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Play the collision sound, if assigned
            if (collisionSound != null)
            {
                collisionSound.Play();
            }

            // Start the process to load the next level after a short delay
            StartCoroutine(LoadNextLevelAfterDelay());
        }
    }

    // Coroutine to introduce a delay before loading the next scene
    private IEnumerator LoadNextLevelAfterDelay()
    {
        // Wait for 0.7 seconds (adjustable delay duration)
        yield return new WaitForSeconds(0.7f);

        // Load the next scene in the build index
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
