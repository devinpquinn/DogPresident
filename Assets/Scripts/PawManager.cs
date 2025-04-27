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

    public RectTransform backgroundRect; // Reference to the background RectTransform
    public float parallaxMaxOffset = 50f; // Maximum offset for the parallax effect
    public float parallaxLerpSpeed = 5f; // Speed of the parallax easing

    public Vector3 restPosition = new Vector3(0f, -3f, 0f); // Define the rest position of the paw
    public Quaternion restRotation = Quaternion.identity; // Define the rest rotation of the paw (default is no rotation)
    private bool isLive = true; // Whether the paw is in live mode

    private Camera mainCamera;
    private bool isSlamming = false; // Whether the arm is currently slamming
    private bool isTracking = true; // Whether the arm is tracking the mouse
    private Vector3 initialLocalPosition; // Initial local position of the child arm
    private Transform childArm; // Reference to the child GameObject (arm sprite)
    private Vector3 targetPosition; // Target position for tracking
    private bool isMovingToClick = false; // Whether the paw is moving to the clicked position
    private Vector2 backgroundInitialPosition; // Store the initial position of the background

    void Start()
    {
        mainCamera = Camera.main;

        // Get the child GameObject (arm sprite) and store its initial local position
        childArm = transform.GetChild(0);
        initialLocalPosition = childArm.localPosition;

        // Store the initial position of the background
        if (backgroundRect != null)
        {
            backgroundInitialPosition = backgroundRect.anchoredPosition;
        }
    }

    void Update()
    {
        if (isSlamming)
            return;

        // Toggle live/rest mode on right-click
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            isLive = !isLive;
            isTracking = isLive; // Enable/disable tracking based on live mode
        }

        if (!isLive)
        {
            // Lerp the paw to the rest position and rest rotation when not live
            transform.position = Vector3.Lerp(transform.position, restPosition, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, restRotation, lerpSpeed * Time.deltaTime);
        }
        else if (isMovingToClick)
        {
            // Move towards the clicked position
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);

            // Check if the paw has reached the clicked position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMovingToClick = false;
                StartCoroutine(Slam());
            }
        }
        else if (isTracking)
        {
            // Get mouse position in world space
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure Z is 0 for 2D

            // Clamp the Y position to the maximum allowed value
            targetPosition = mousePosition;
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

        // Check for left-click to initiate slam
        if (Input.GetMouseButtonDown(0) && !isSlamming)
        {
            isMovingToClick = true;
            isTracking = false; // Temporarily disable tracking during the slam
        }

        // Parallax effect (always active)
        if (backgroundRect != null)
        {
            // Get the normalized horizontal position of the cursor (-1 to 1)
            float normalizedCursorX = (Input.mousePosition.x / Screen.width) * 2f - 1f;

            // Calculate the target offset based on the normalized cursor position
            float targetOffsetX = normalizedCursorX * parallaxMaxOffset * -1f;

            // Smoothly lerp the background's position to the target offset
            Vector2 targetPosition = new Vector2(backgroundInitialPosition.x + targetOffsetX, backgroundInitialPosition.y);
            backgroundRect.anchoredPosition = Vector2.Lerp(backgroundRect.anchoredPosition, targetPosition, parallaxLerpSpeed * Time.deltaTime);

            // Apply a parallax effect to the child paw's global X offset
            float childParallaxOffset = normalizedCursorX * (parallaxMaxOffset * 0.2f); // Adjust the multiplier for subtle movement
            Vector3 childGlobalPosition = childArm.position;
            childGlobalPosition.x = transform.position.x + childParallaxOffset; // Offset relative to the parent
            childArm.position = childGlobalPosition;
        }
    }

    private IEnumerator Slam()
    {
        isSlamming = true;

        // Store the current local position of the child arm as the return position
        Vector3 returnPosition = childArm.localPosition;

        // Windup: Move the child arm slightly back and scale up
        Vector3 windupPosition = returnPosition * windupMultiplier; // Use the windup multiplier
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

        // Lerp the child arm back to its return position and scale up to the default scale
        Vector3 initialScale = Vector3.one * 1.1f; // Default scale of the child arm
        while (Vector3.Distance(childArm.localPosition, returnPosition) > 0.01f || Vector3.Distance(childArm.localScale, initialScale) > 0.01f)
        {
            childArm.localPosition = Vector3.Lerp(childArm.localPosition, returnPosition, slamSpeed * Time.deltaTime);
            childArm.localScale = Vector3.Lerp(childArm.localScale, initialScale, slamSpeed * Time.deltaTime);
            yield return null;
        }
        // Ensure exact values after returning to the return position
        childArm.localPosition = returnPosition;
        childArm.localScale = initialScale;

        // Reset state
        isSlamming = false;
        isTracking = true; // Re-enable tracking after the slam
    }
}
