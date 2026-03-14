using TMPro;
using UnityEngine;
using System.Collections;

public class IssueManager : MonoBehaviour
{
    public TextMeshProUGUI issueText;
    private CanvasGroup issueCanvasGroup;
    public float fadeDuration = 0.3f;
    public float slideDistanceX = 300f;
    private AudioSource audioSource;
    public AudioClip issueAppearSound;
    private RectTransform issueRectTransform;
    private Vector2 baseAnchoredPosition;

    private void Awake()
    {
        issueCanvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
        issueRectTransform = GetComponent<RectTransform>();

        if (issueCanvasGroup == null && issueText != null)
        {
            issueCanvasGroup = issueText.GetComponentInParent<CanvasGroup>();
        }

        if (issueRectTransform == null && issueText != null)
        {
            issueRectTransform = issueText.GetComponentInParent<RectTransform>();
        }

        if (issueRectTransform != null)
        {
            baseAnchoredPosition = issueRectTransform.anchoredPosition;
        }

        if (issueCanvasGroup != null)
        {
            issueCanvasGroup.alpha = 0f;
        }
    }
    
    public IEnumerator DisplayIssue(string issue)
    {
        issueText.text = issue;
        if (audioSource != null && issueAppearSound != null)
        {
            audioSource.PlayOneShot(issueAppearSound);
        }

        SetAnchoredPositionX(baseAnchoredPosition.x - slideDistanceX);

        yield return FadeAndSlide(1f, baseAnchoredPosition.x);
    }
    
    public IEnumerator HideIssue()
    {
        yield return FadeAndSlide(0f, GetCurrentAnchoredPositionX());
        issueText.text = "";
    }

    private IEnumerator FadeAndSlide(float targetAlpha, float targetX)
    {
        if (issueCanvasGroup == null)
        {
            yield break;
        }

        if (fadeDuration <= 0f)
        {
            issueCanvasGroup.alpha = targetAlpha;
            SetAnchoredPositionX(targetX);
            yield break;
        }

        float startAlpha = issueCanvasGroup.alpha;
        float startX = GetCurrentAnchoredPositionX();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out for smoother stop.
            issueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAnchoredPositionX(Mathf.Lerp(startX, targetX, easedT));
            yield return null;
        }

        issueCanvasGroup.alpha = targetAlpha;
        SetAnchoredPositionX(targetX);
    }

    private float GetCurrentAnchoredPositionX()
    {
        if (issueRectTransform == null)
        {
            return 0f;
        }

        return issueRectTransform.anchoredPosition.x;
    }

    private void SetAnchoredPositionX(float x)
    {
        if (issueRectTransform == null)
        {
            return;
        }

        Vector2 position = issueRectTransform.anchoredPosition;
        position.x = x;
        issueRectTransform.anchoredPosition = position;
    }
}
