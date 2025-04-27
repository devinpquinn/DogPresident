using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawManager : MonoBehaviour
{
    public float lerpSpeed = 5f; // Speed of lerping
    public float maxY = 5f; // Maximum Y position
    public float rotationFactor = 10f; // Factor to control rotation sensitivity
    public float rotationBias = 0.66f; // Bias for the middle point of no rotation (2/3 of the screen width)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure Z is 0 for 2D

        // Clamp the Y position to the maximum allowed value
        Vector3 targetPosition = mousePosition;
        targetPosition.y = Mathf.Min(targetPosition.y, maxY);

        // Lerp the object's position towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

        // Calculate rotation based on horizontal distance to the mouse
        float screenWidth = Screen.width;
        float screenMiddle = screenWidth * rotationBias; // Adjust middle point based on bias
        float horizontalDistance = (Input.mousePosition.x - screenMiddle) / screenWidth;

        // Apply rotation with bias
        float rotationZ = -horizontalDistance * rotationFactor;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }
}
