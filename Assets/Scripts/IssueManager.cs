using TMPro;
using UnityEngine;
using System.Collections;

public class IssueManager : MonoBehaviour
{
    public TextMeshProUGUI issueText;
    private CanvasGroup issueCanvasGroup;
    private float fadeDuration = 0.5f;

    private void Awake()
    {
        issueCanvasGroup = GetComponent<CanvasGroup>();

        if (issueCanvasGroup == null && issueText != null)
        {
            issueCanvasGroup = issueText.GetComponentInParent<CanvasGroup>();
        }

        if (issueCanvasGroup != null)
        {
            issueCanvasGroup.alpha = 0f;
        }
    }
    
    public IEnumerator DisplayIssue(string issue)
    {
        issueText.text = issue;
        yield return FadeCanvasGroup(1f);
    }
    
    public IEnumerator HideIssue()
    {
        yield return FadeCanvasGroup(0f);
        issueText.text = "";
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        if (issueCanvasGroup == null)
        {
            yield break;
        }

        float startAlpha = issueCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            issueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        issueCanvasGroup.alpha = targetAlpha;
    }
}
