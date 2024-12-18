using UnityEngine;
using UnityEngine.SceneManagement;

public class Camera : MonoBehaviour
{
    // The object the camera will follow
    public GameObject targetObject;

    // Offset values to position the camera relative to the target
    public float xOffset, yOffset, zOffset;

    private void Start()
    {
        // Make camera persist between scenes
        DontDestroyOnLoad(gameObject);

        // If target not assigned, try to find player
        if (targetObject == null)
        {
            targetObject = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void OnEnable()
    {
        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene load event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If target is lost, try to find player again
        if (targetObject == null)
        {
            targetObject = GameObject.FindGameObjectWithTag("Player");
        }
    }

    void Update()
    {
        if (targetObject != null)
        {
            // Set the camera's position relative to the target object with the specified offsets
            transform.position = targetObject.transform.position + new Vector3(xOffset, yOffset, zOffset);

            // Rotate the camera to look at the target object
            transform.LookAt(targetObject.transform.position);
        }
        else
        {
            Debug.LogWarning("Camera target object is missing!");
        }
    }
}