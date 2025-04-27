using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawManager : MonoBehaviour
{
    public float lerpSpeed = 5f; // Speed of lerping
    public float maxY = 5f; // Maximum Y position
    public float rotationFactor = 10f; // Factor to control rotation sensitivity
    public float rotationBias = 0.66f; // Bias for the middle point of no rotation (2/3 of the screen width)
    public float slamSpeed = 10f; // Speed of the slam
    public float slamHoldTime = 0.5f; // Time to hold the slam position
    public float windupMultiplier = 1.25f; // Multiplier for the windup offset

    private Camera mainCamera;
    private bool isSlamming = false; // Whether the arm is currently slamming
    private bool isTracking = true; // Whether the arm is tracking the mouse
    private Vector3 initialLocalPosition; // Initial local position of the child arm
    private Transform childArm; // Reference to the child GameObject (arm sprite)
    private Vector3 targetPosition; // Target position for tracking
    private bool isMovingToClick = false; // Whether the paw is moving to the clicked position

    void Start()
    {
        mainCamera = Camera.main;

        // Get the child GameObject (arm sprite) and store its initial local position
        childArm = transform.GetChild(0);
        initialLocalPosition = childArm.localPosition;
    }

    void Update()
    {
        if (isSlamming)
            return;

        if (isMovingToClick)
        {
            // Move towards the clicked position
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

            // Check if the paw has reached the clicked position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMovingToClick = false;
                StartCoroutine(Slam());
            }
            return;
        }

        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure Z is 0 for 2D

        // Clamp the Y position to the maximum allowed value
        targetPosition = mousePosition;
        targetPosition.y = Mathf.Min(targetPosition.y, maxY);

        // Lerp the object's position towards the target position
        if (isTracking)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

            // Calculate rotation based on horizontal distance to the mouse
            float screenWidth = Screen.width;
            float screenMiddle = screenWidth * rotationBias; // Adjust middle point based on bias
            float horizontalDistance = (Input.mousePosition.x - screenMiddle) / screenWidth;

            // Apply rotation with bias
            float rotationZ = -horizontalDistance * rotationFactor;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        }

        // Check for mouse click to initiate slam
        if (Input.GetMouseButtonDown(0))
        {
            isMovingToClick = true;
            isTracking = false;
        }
    }

    private IEnumerator Slam()
    {
        isSlamming = true;

        // Windup: Move the child arm slightly back and scale up
        Vector3 windupPosition = initialLocalPosition * windupMultiplier; // Use the windup multiplier
        Vector3 windupScale = Vector3.one * (1.1f * windupMultiplier); // Scale up during windup
        while (Vector3.Distance(childArm.localPosition, windupPosition) > 0.01f || Vector3.Distance(childArm.localScale, windupScale) > 0.01f)
        {
            childArm.localPosition = Vector3.Lerp(childArm.localPosition, windupPosition, slamSpeed * Time.deltaTime);
            childArm.localScale = Vector3.Lerp(childArm.localScale, windupScale, slamSpeed * Time.deltaTime);
            yield return null;
        }
        // Ensure exact values after windup
        childArm.localPosition = windupPosition;
        childArm.localScale = windupScale;

        // Lerp the child arm to local position zero (slam position) and scale down to 1
        Vector3 slamPosition = Vector3.zero;
        Vector3 slamScale = Vector3.one; // Scale down to 1 during the slam
        while (Vector3.Distance(childArm.localPosition, slamPosition) > 0.01f || Vector3.Distance(childArm.localScale, slamScale) > 0.01f)
        {
            childArm.localPosition = Vector3.Lerp(childArm.localPosition, slamPosition, slamSpeed * Time.deltaTime);
            childArm.localScale = Vector3.Lerp(childArm.localScale, slamScale, slamSpeed * Time.deltaTime);
            yield return null;
        }
        // Ensure exact values after slam
        childArm.localPosition = slamPosition;
        childArm.localScale = slamScale;

        // Hold the slam position for a moment
        yield return new WaitForSeconds(slamHoldTime);

        // Lerp the child arm back to its initial local position and scale up to the default scale
        Vector3 initialScale = Vector3.one * 1.1f; // Default scale of the child arm
        while (Vector3.Distance(childArm.localPosition, initialLocalPosition) > 0.01f || Vector3.Distance(childArm.localScale, initialScale) > 0.01f)
        {
            childArm.localPosition = Vector3.Lerp(childArm.localPosition, initialLocalPosition, slamSpeed * Time.deltaTime);
            childArm.localScale = Vector3.Lerp(childArm.localScale, initialScale, slamSpeed * Time.deltaTime);
            yield return null;
        }
        // Ensure exact values after returning to initial position
        childArm.localPosition = initialLocalPosition;
        childArm.localScale = initialScale;

        // Reset state
        isSlamming = false;
        isTracking = true;
    }
}
