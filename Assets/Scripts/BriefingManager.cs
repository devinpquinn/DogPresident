using System.Collections;
using UnityEngine;

public class BriefingManager : MonoBehaviour
{
    [Header("Slide In Settings")]
    public RectTransform targetRectTransform;
    public Vector3 targetPosition;
    public Vector3 positionVariance;
    public Vector3 targetRotation;
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

        // Calculate the final target position and rotation with variance
        Vector3 finalPosition = targetPosition + new Vector3(
            Random.Range(-positionVariance.x, positionVariance.x),
            Random.Range(-positionVariance.y, positionVariance.y),
            Random.Range(-positionVariance.z, positionVariance.z)
        );

        Quaternion finalRotation = Quaternion.Euler(targetRotation + new Vector3(
            Random.Range(-rotationVariance.x, rotationVariance.x),
            Random.Range(-rotationVariance.y, rotationVariance.y),
            Random.Range(-rotationVariance.z, rotationVariance.z)
        ));

        while (lerpTime < lerpDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / lerpDuration;

            // Lerp position and rotation
            targetRectTransform.localPosition = Vector3.Lerp(initialPosition, finalPosition, t);
            targetRectTransform.localRotation = Quaternion.Lerp(initialRotation, finalRotation, t);

            yield return null;
        }

        // Ensure final position and rotation are set
        targetRectTransform.localPosition = finalPosition;
        targetRectTransform.localRotation = finalRotation;

        isSlidingIn = false;
        lerpTime = 0f;
    }
}
