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

    / Class for background music options
    [System.Serializable]
    public class BackgroundMusic
    {
        public string trackName; // Name of the track to display in the UI
        public AudioClip music; // The actual audio clip
        public bool isDefault; // Indicates if this track is the default one to play
    }

    // Array of background music options
    public BackgroundMusic[] backgroundTracks;
    private AudioSource backgroundSource; // AudioSource component for background music
    private int currentTrackIndex = 0; // Index of the currently playing background track
    private int enemyKillCount = 0; // Count of enemies killed by the player

    // UI element for music selection dropdown
    public TMP_Dropdown musicDropdown; // Dropdown UI for selecting background music

    void Start()    
    {
        // Setup background music and music selection dropdown
        SetupBackgroundMusic();
        SetupMusicDropdown();
    }

        // Setup background music options
    void SetupBackgroundMusic()
    {
        // Create and configure the AudioSource for background music
        backgroundSource = gameObject.AddComponent<AudioSource>();
        backgroundSource.loop = true; // Set to loop the music
        backgroundSource.volume = 0.3f; // Set the volume level
        backgroundSource.playOnAwake = true; // Start playing immediately if there is a track

        // Find and play the default track, if one is set
        for (int i = 0; i < backgroundTracks.Length; i++)
        {
            if (backgroundTracks[i].isDefault)
            {
                currentTrackIndex = i; // Store the index of the default track
                break; // Exit the loop once found
            }
        }

        // If no default is set, play the first track
        if (backgroundTracks.Length > 0)
        {
            PlayBackgroundTrack(currentTrackIndex);
        }
    }

    // Setup the music selection dropdown UI
    void SetupMusicDropdown()
    {
        if (musicDropdown != null) // Ensure the dropdown is assigned
        {
            musicDropdown.ClearOptions(); // Clear any existing options

            // Create a list of track names for the dropdown
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (var track in backgroundTracks)
            {
                options.Add(new TMP_Dropdown.OptionData(track.trackName)); // Add track names as options
            }

            musicDropdown.AddOptions(options); // Add options to the dropdown
            musicDropdown.value = currentTrackIndex; // Set the current track as the selected option
            musicDropdown.onValueChanged.AddListener(OnMusicSelectionChanged); // Register event listener
        }
    }

    // Event triggered when a new music track is selected from the dropdown
    public void OnMusicSelectionChanged(int index)
    {
        PlayBackgroundTrack(index); // Play the selected track
    }

    // Play the selected background track
    void PlayBackgroundTrack(int index)
    {
        if (index >= 0 && index < backgroundTracks.Length) // Ensure the index is valid
        {
            currentTrackIndex = index; // Update the current track index
            backgroundSource.clip = backgroundTracks[index].music; // Set the audio clip to the selected track
            backgroundSource.Play(); // Play the selected track
        }
    }

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
        // Retrieve horizontal (A/D or arrow keys) and vertical (W/S or arrow keys) input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    // Applies force to the Rigidbody based on input
    private void ApplyMotion()
    {
        // Create a movement vector and apply force scaled by the motion speed
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * motionSpeed;
        rb.AddForce(movement);
    }
}
