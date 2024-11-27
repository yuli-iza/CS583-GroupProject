using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetObject : MonoBehaviour
{
    // Boundary value to check if the object has fallen off the scene
    public float boundary = -20f;

    // Called once per frame to check object position
    void Update()
    {
        // If the object's Y-position is below the boundary, reset the scene
        if (transform.position.y < boundary)
        {
            // Reload the current scene using its build index
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
