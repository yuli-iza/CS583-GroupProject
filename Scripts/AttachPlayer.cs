using UnityEngine;

public class AttachPlayer : MonoBehaviour
{
    private GameObject Player;
    private Vector3 lastPlatformPosition;
    private bool isPlayerAttached = false;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (Player == null)
        {
            Debug.LogError("Player not found! Ensure the Player has the 'Player' tag.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            lastPlatformPosition = transform.position;
            isPlayerAttached = true;
            Debug.Log("Player attached to platform: " + transform.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Player)
        {
            isPlayerAttached = false;
            Debug.Log("Player detached from platform: " + transform.name);
        }
    }

    private void LateUpdate()
    {
        if (isPlayerAttached && Player != null)
        {
            // Calculate how much the platform has moved since last frame
            Vector3 platformDelta = transform.position - lastPlatformPosition;

            // Move the player by the same amount
            Player.transform.position += platformDelta;

            // Update the last position for next frame
            lastPlatformPosition = transform.position;
        }
    }
}