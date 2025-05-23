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
        // Randomize this GameObject's Z rotation between -10 and 10 degrees
        float randomZ = Random.Range(-10f, 10f);
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            randomZ
        );

        if (newspaperRect != null)
        {
            newspaperRect.anchoredPosition = startOffset;
            newspaperRect.localScale = startScale;
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

        // Randomize this GameObject's Z rotation between -10 and 10 degrees each time animation starts
        float randomZ = Random.Range(-10f, 10f);
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            randomZ
        );

        float elapsed = 0f;
        Vector2 initialPos = startOffset;
        Vector2 targetPos = Vector2.zero;
        Vector3 initialScale = startScale;
        Vector3 targetScale = Vector3.one;
        Vector3 shadowInitialScale = shadowStartScale;
        Vector3 shadowTargetScale = Vector3.one;

        // Shadow offset is half of the newspaper offset
        Vector2 shadowInitialPos = startOffset * 0.25f;
        Vector2 shadowTargetPos = Vector2.zero;

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
            float easeT = t * t; // Ease-in: starts slow, accelerates

            newspaperRect.anchoredPosition = Vector2.Lerp(initialPos, targetPos, easeT);
            newspaperRect.localScale = Vector3.Lerp(initialScale, targetScale, easeT);

            if (shadowRect != null)
            {
                shadowRect.anchoredPosition = Vector2.Lerp(shadowInitialPos, shadowTargetPos, easeT);
                shadowRect.localScale = Vector3.Lerp(shadowInitialScale, shadowTargetScale, easeT);
                if (shadowImage != null)
                {
                    Color c = Color.Lerp(shadowInitialColor, shadowTargetColor, easeT);
                    shadowImage.color = c;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        newspaperRect.anchoredPosition = targetPos;
        newspaperRect.localScale = targetScale;
        if (shadowRect != null)
        {
            shadowRect.anchoredPosition = shadowTargetPos;
            shadowRect.localScale = shadowTargetScale;
            if (shadowImage != null)
            {
                shadowImage.color = shadowTargetColor;
            }
        }
        isAnimating = false;
    }
}
