using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // The object the camera will follow
    public GameObject targetObject;

    // Offset values to position the camera relative to the target
    public float xOffset, yOffset, zOffset;

    // Called once per frame to update the camera's position
    void Update()
    {
        // Set the camera's position relative to the target object with the specified offsets
        transform.position = targetObject.transform.position + new Vector3(xOffset, yOffset, zOffset);

        // Rotate the camera to look at the target object
        transform.LookAt(targetObject.transform.position);
    }
}
