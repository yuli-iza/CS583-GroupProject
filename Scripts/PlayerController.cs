using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Rigidbody component for controlling physics-based movement
    public Rigidbody rb;
    // Speed multiplier for player motion
    public float motionSpeed = 10f;
    // Private variables to store input values
    private float horizontalInput;
    private float verticalInput;
    // Called before the first frame update
    void Awake()
    {
        // Get and store the Rigidbody component attached to the player
        rb = GetComponent<Rigidbody>();
    }
    // Called once per frame to handle user input
    void Update()
    {
        HandleInput();
    }
    // Called at a fixed interval to apply physics updates
    void FixedUpdate()
    {
        ApplyMotion();
    }
    // Handles player input for movement
    private void HandleInput()
    {
        // Only get input if game is playing
        if (GameManager.Instance.isPlaying)
        {
            // Retrieve horizontal (A/D or arrow keys) and vertical (W/S or arrow keys) input
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }
        else
        {
            // Reset input values when not playing
            horizontalInput = 0f;
            verticalInput = 0f;
        }
    }
    // Applies force to the Rigidbody based on input
    private void ApplyMotion()
    {
        // Create a movement vector and apply force scaled by the motion speed
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * motionSpeed;
        rb.AddForce(movement);
    }
}