using System.Collections;
using UnityEngine;

public class BriefingManager : MonoBehaviour
{
    [Header("Slide In Settings")]
    public RectTransform targetRectTransform;
    public Vector3 targetPosition;
    public Vector3 rotationVariance;
    public float lerpDuration = 1.0f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isSlidingIn = false;
    private float lerpTime = 0f;
    
    public GameObject folderClosed;
    public GameObject folderOpen;
    public float waitToOpen = 0.75f;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Single AudioSource for both sounds
    public AudioClip slideSoundEffect; // Clip for sliding
    public AudioClip folderOpenSoundEffect; // Clip for folder opening

    void Start()
    {
        if (targetRectTransform != null)
        {
            initialPosition = targetRectTransform.localPosition;
            initialRotation = targetRectTransform.localRotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSlidingIn)
        {
            StartCoroutine(SlideIn());
        }
    }

    private IEnumerator SlideIn()
    {
        isSlidingIn = true;

        // Play the slide sound effect as a one-shot
        if (audioSource != null && slideSoundEffect != null)
        {
            audioSource.PlayOneShot(slideSoundEffect);
        }

        // Ensure folder is closed
        folderClosed.SetActive(true);
        folderOpen.SetActive(false);

        // Use the initial position as the start position
        Vector3 startPosition = initialPosition;

        // Use the target position directly as the final position
        Vector3 finalPosition = targetPosition;

        // Calculate the final target rotation with variance
        Quaternion finalRotation = Quaternion.Euler(new Vector3(
            Random.Range(-rotationVariance.x, rotationVariance.x),
            Random.Range(-rotationVariance.y, rotationVariance.y),
            Random.Range(-rotationVariance.z, rotationVariance.z)
        ));

        while (lerpTime < lerpDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / lerpDuration;

            // Apply a custom cubic ease-out effect
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            // Lerp position and rotation with easedT
            targetRectTransform.localPosition = Vector3.Lerp(startPosition, finalPosition, easedT);
            targetRectTransform.localRotation = Quaternion.Lerp(initialRotation, finalRotation, easedT);

            yield return null;
        }

        // Ensure final position and rotation are set
        targetRectTransform.localPosition = finalPosition;
        targetRectTransform.localRotation = finalRotation;

        isSlidingIn = false;
        lerpTime = 0f;

        // Wait for a moment before opening the folder
        yield return new WaitForSeconds(waitToOpen);

        // Play the folder open sound effect as a one-shot
        if (audioSource != null && folderOpenSoundEffect != null)
        {
            audioSource.PlayOneShot(folderOpenSoundEffect);
        }

        folderClosed.SetActive(false);
        folderOpen.SetActive(true);
    }
}
