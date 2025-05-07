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

            // Apply ease-out effect using Mathf.SmoothStep
            float easedT = Mathf.SmoothStep(0f, 1f, t);

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
    }
}
