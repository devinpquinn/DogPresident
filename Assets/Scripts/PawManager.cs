using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawManager : MonoBehaviour
{
    public float lerpSpeed = 5f; // Speed of lerping
    public float maxY = -2.2f; // Maximum Y position
    public float rotationFactor = 30f; // Factor to control rotation sensitivity
    public float rotationBias = 0.55f; // Bias for the middle point of no rotation
    public float slamSpeed = 50f; // Speed of the slam
    public float slamHoldTime = 0.1f; // Time to hold the slam position
    public float windupMultiplier = 1.25f; // Multiplier for the windup offset
    public float slamCompletionPercentageButton = 0.8f; // Percentage of the slam to complete when hitting a button
    public float buttonSlamHoldTime = 1f; // Time to hold the slam position when hitting a button
    public float pitchVariation = 0.1f; // Amount of pitch variation

    public RectTransform backgroundRect; // Reference to the background RectTransform
    public float parallaxMaxOffset = 50f; // Maximum offset for the parallax effect
    public float parallaxLerpSpeed = 5f; // Speed of the parallax easing

    public AudioClip carpetHitClip; // Audio clip for hitting the carpet
    public AudioClip buttonDownClip; // Audio clip for pressing a button
    public AudioClip buttonUpClip; // Audio clip for releasing a button
    private AudioSource audioSource; // Reference to the AudioSource component

    public SpriteRenderer mainSpriteRenderer; // Assign in Inspector
    public SpriteRenderer shadowSpriteRenderer; // Assign in Inspector
    public Sprite mainUpSprite;
    public Sprite mainDownSprite;
    public Sprite shadowUpSprite;
    public Sprite shadowDownSprite;

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

        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
            // Get the normalized horizontal position of the cursor (-1 to 1), accounting for rotationBias
            float screenWidth = Screen.width;
            float screenMiddle = screenWidth * rotationBias; // Adjust middle point based on bias
            float normalizedCursorX = (Input.mousePosition.x - screenMiddle) / screenWidth * 2f;

            // Calculate the target offset based on the normalized cursor position
            float targetOffsetX = normalizedCursorX * parallaxMaxOffset * -1f;

            // Smoothly lerp the background's position to the target offset
            Vector2 targetPosition = new Vector2(backgroundInitialPosition.x + targetOffsetX, backgroundInitialPosition.y);
            backgroundRect.anchoredPosition = Vector2.Lerp(backgroundRect.anchoredPosition, targetPosition, parallaxLerpSpeed * Time.deltaTime);

            // Apply a parallax effect to the child paw's global X offset relative to its base offset
            float childParallaxOffset = normalizedCursorX * (parallaxMaxOffset * 0.05f); // Adjust the multiplier for subtle movement

            // Transform the local base offset to global space
            Vector3 baseGlobalPosition = transform.TransformPoint(initialLocalPosition);

            // Apply the parallax offset to the global position
            Vector3 childGlobalPosition = baseGlobalPosition;
            childGlobalPosition.x += childParallaxOffset; // Add the parallax offset to the global X position

            // Set the child arm's global position
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

        // Perform raycast at the start of the slam
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0f, LayerMask.GetMask("Button"));
        bool willHitButton = hit.collider != null;

        // Determine the slam target position based on whether a button will be hit
        Vector3 slamPosition = Vector3.zero; // 80% of the way down if hitting a button
        Vector3 slamScale = Vector3.one; // Scale down to 1 during the slam
        
        // Change to "down" sprites
        if (mainSpriteRenderer != null && mainDownSprite != null)
            mainSpriteRenderer.sprite = mainDownSprite;
        if (shadowSpriteRenderer != null && shadowDownSprite != null)
            shadowSpriteRenderer.sprite = shadowDownSprite;

        // Lerp the child arm to the slam position and scale
        while (Vector3.Distance(childArm.localPosition, slamPosition) > 0.01f || Vector3.Distance(childArm.localScale, slamScale) > 0.01f)
        {
            if (willHitButton)
            {
                // Calculate the partial slam position and scale based on the slamCompletionPercentage
                Vector3 partialSlamPosition = Vector3.Lerp(returnPosition, slamPosition, slamCompletionPercentageButton);
                Vector3 partialSlamScale = Vector3.Lerp(Vector3.one * 1.1f, slamScale, slamCompletionPercentageButton);

                // Lerp towards the partial slam position and scale
                childArm.localPosition = Vector3.Lerp(childArm.localPosition, partialSlamPosition, slamSpeed * Time.deltaTime);
                childArm.localScale = Vector3.Lerp(childArm.localScale, partialSlamScale, slamSpeed * Time.deltaTime);

                // Break out of the loop once close enough to the partial position and scale
                if (Vector3.Distance(childArm.localPosition, partialSlamPosition) <= 0.01f && Vector3.Distance(childArm.localScale, partialSlamScale) <= 0.01f)
                {
                    childArm.localPosition = partialSlamPosition;
                    childArm.localScale = partialSlamScale;
                    break;
                }
            }
            else
            {
                // Normal slam behavior
                childArm.localPosition = Vector3.Lerp(childArm.localPosition, slamPosition, slamSpeed * Time.deltaTime);
                childArm.localScale = Vector3.Lerp(childArm.localScale, slamScale, slamSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Play the appropriate audio clip based on whether a button was hit
        if (willHitButton)
        {
            if (buttonDownClip != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation); // Apply pitch variation
                audioSource.PlayOneShot(buttonDownClip);
            }
        }
        else
        {
            if (carpetHitClip != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation); // Apply pitch variation
                audioSource.PlayOneShot(carpetHitClip);
            }
        }

        // If a button will be hit, disable its sprite renderer after the slam
        if (willHitButton)
        {
            StartCoroutine(DisableTemporarily(hit.collider.gameObject, buttonSlamHoldTime));
        }

        // Hold the slam position for a moment
        yield return new WaitForSeconds(willHitButton ? buttonSlamHoldTime : slamHoldTime);

        // Play the button up clip when releasing the hold
        if (willHitButton && buttonUpClip != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation); // Apply pitch variation
            audioSource.PlayOneShot(buttonUpClip);
        }

        // Revert to "up" sprites
        if (mainSpriteRenderer != null && mainUpSprite != null)
            mainSpriteRenderer.sprite = mainUpSprite;
        if (shadowSpriteRenderer != null && shadowUpSprite != null)
            shadowSpriteRenderer.sprite = shadowUpSprite;

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

    private IEnumerator DisableTemporarily(GameObject target, float duration)
    {
        target.SetActive(false);
        yield return new WaitForSeconds(duration);
        target.SetActive(true);
    }
}
