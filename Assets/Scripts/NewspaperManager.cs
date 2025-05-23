using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperManager : MonoBehaviour
{
    [Header("Assign the RectTransform of the newspaper UI")]
    public RectTransform newspaperRect;
    [Header("Assign the RectTransform of the newspaper shadow")]
    public RectTransform shadowRect;
    public Vector2 startOffset = new Vector2(0, -800); // Example offset (off-screen below)
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 startRotation = new Vector3(0, 0, 30f); // Example: 30 degrees Z rotation
    public Vector3 shadowStartScale = new Vector3(0.5f, 0.5f, 1f);
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
        if (shadowRect != null)
        {
            shadowRect.localScale = shadowStartScale;
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
        Vector3 shadowInitialScale = shadowStartScale;
        Vector3 shadowTargetScale = Vector3.one;

        Image shadowImage = shadowRect != null ? shadowRect.GetComponent<Image>() : null;
        Color shadowInitialColor = shadowImage != null ? shadowImage.color : Color.black;
        shadowInitialColor.a = 0f;
        Color shadowTargetColor = shadowInitialColor;
        shadowTargetColor.a = 1f;
        if (shadowImage != null)
            shadowImage.color = shadowInitialColor;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            newspaperRect.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);
            newspaperRect.localScale = Vector3.Lerp(initialScale, targetScale, t);
            Vector3 currentRot = Vector3.Lerp(initialRot, targetRot, t);
            newspaperRect.localEulerAngles = currentRot;

            if (shadowRect != null)
            {
                shadowRect.localScale = Vector3.Lerp(shadowInitialScale, shadowTargetScale, t);
                shadowRect.localEulerAngles = currentRot;
                if (shadowImage != null)
                {
                    Color c = Color.Lerp(shadowInitialColor, shadowTargetColor, t);
                    shadowImage.color = c;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        newspaperRect.anchoredPosition = targetPos;
        newspaperRect.localScale = targetScale;
        newspaperRect.localEulerAngles = targetRot;
        if (shadowRect != null)
        {
            shadowRect.localScale = shadowTargetScale;
            shadowRect.localEulerAngles = targetRot;
            if (shadowImage != null)
            {
                shadowImage.color = shadowTargetColor;
            }
        }
        isAnimating = false;
    }
}
