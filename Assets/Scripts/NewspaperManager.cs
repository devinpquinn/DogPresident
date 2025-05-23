using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewspaperManager : MonoBehaviour
{
    [Header("Assign the RectTransform of the newspaper UI")]
    public RectTransform newspaperRect;
    public Vector2 startOffset = new Vector2(0, -800); // Example offset (off-screen below)
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 startRotation = new Vector3(0, 0, 30f); // Example: 30 degrees Z rotation
    public float animationDuration = 0.5f;

    private bool isAnimating = false;

    void Start()
    {
        if (newspaperRect != null)
        {
            newspaperRect.anchoredPosition = startOffset;
            newspaperRect.localScale = startScale;
            newspaperRect.localEulerAngles = startRotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !isAnimating)
        {
            StartCoroutine(AnimateNewspaperIn());
        }
    }

    IEnumerator AnimateNewspaperIn()
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector2 initialPos = startOffset;
        Vector2 targetPos = Vector2.zero;
        Vector3 initialScale = startScale;
        Vector3 targetScale = Vector3.one;
        Vector3 initialRot = startRotation;
        Vector3 targetRot = Vector3.zero;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            newspaperRect.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);
            newspaperRect.localScale = Vector3.Lerp(initialScale, targetScale, t);
            newspaperRect.localEulerAngles = Vector3.Lerp(initialRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        newspaperRect.anchoredPosition = targetPos;
        newspaperRect.localScale = targetScale;
        newspaperRect.localEulerAngles = targetRot;
        isAnimating = false;
    }
}
