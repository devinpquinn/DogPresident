using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;

public class GameController : MonoBehaviour
{
    public PawManager pawManager;
    public ScenarioManager scenarioManager;
    public IssueManager issueManager;
    public NewspaperManager newspaperManager;
    public UnityEngine.UI.Image backgroundImage;
    public float hueLerpDuration = 1f;
    public Color minApprovalColor = new Color(0.8f, 0.2f, 0.2f);
    public Color maxApprovalColor = new Color(0.2f, 0.8f, 0.2f);
    public TextMeshProUGUI headerText;
    public GameObject gameOverPopup;
    public TextMeshProUGUI gameOverHeaderText;
    public TextMeshProUGUI gameOverSubheaderText;
    public RectTransform graphArea;
    public float graphLineWidth = 6f;
    public float graphAnimDuration = 1f;
    public Material graphLineMaterial;
    public Button restartButton;
    private int turnNumber = 1;

    private Scenario currentScenario;

    private bool waitingForSlam = false;
    private bool waitingForNewspaperClick = false;
    private bool isGameOver = false;

    private int approvalRating = 50; // Start at 50%
    private readonly List<int> approvalHistory = new List<int>();
    private GameObject graphLineObject;

    private System.Random rng = new System.Random();

    private readonly string[] positiveSmallPhrases = {
        "Approval rating improves",
        "Approval rating rises",
        "Approval rating ticks up"
    };
    private readonly string[] positiveLargePhrases = {
        "Approval rating soars",
        "Approval rating skyrockets",
        "Approval rating jumps"
    };
    private readonly string[] negativeSmallPhrases = {
        "Approval rating drops",
        "Approval rating dips",
        "Approval rating falls slightly"
    };
    private readonly string[] negativeLargePhrases = {
        "Approval rating plummets",
        "Approval rating nosedives",
        "Approval rating crashes"
    };

    void Start()
    {
        // Subscribe to scenario and response events
        scenarioManager.onScenarioChanged += OnScenarioChanged;
        scenarioManager.onResponsePlayed += OnResponsePlayed;

        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        // Start the loop
        StartCoroutine(GameplayLoop());

        ResetApprovalHistory();
        InitializeBackgroundHue();
        UpdateHeaderText();
    }

    IEnumerator GameplayLoop()
    {
        while (true)
        {
            // 1. Paw at rest, not live or tracking
            pawManager.SetLive(false);

            // 2. Select a scenario (advance)
            scenarioManager.AdvanceToNextScenario();

            // Wait for scenario to be loaded and event to fire
            yield return new WaitUntil(() => currentScenario != null);

            // 3. Fade in the issue text for the current scenario
            yield return StartCoroutine(issueManager.DisplayIssue(currentScenario.promptText));

            // 4. Set paw live and tracking
            pawManager.SetLive(true);

            // 5. Wait for player to slam a button (wait for slam and get index)
            int chosenResponse = -1;
            waitingForSlam = true;
            pawManager.OnButtonSlammed = (index) => { chosenResponse = index; waitingForSlam = false; };
            yield return new WaitUntil(() => !waitingForSlam);

            // Wait for the slam animation (including return) to finish
            yield return StartCoroutine(pawManager.WaitForSlamComplete());

            // 6. Set paw not live or tracking, then fade out the issue text
            pawManager.SetLive(false);
            StartCoroutine(issueManager.HideIssue());
            
            // 6.5. Wait a moment for the issue to fade out before showing the newspaper
            yield return new WaitForSeconds(0.25f);

            // 7. Show newspaper with result
            Response response = currentScenario.responses[chosenResponse];

            // Apply approval effect
            int delta = GetApprovalDelta(response.approvalEffect);
            approvalRating = Mathf.Clamp(approvalRating + delta, 0, 100);
            approvalHistory.Add(approvalRating);

            string phrase = GetApprovalPhrase(response.approvalEffect, delta);
            newspaperManager.headlineText.text = response.headline;
            newspaperManager.subheadingText.text = response.subheading;
            newspaperManager.approvalRatingText.text = $"{phrase} to {approvalRating} percent";
            StartCoroutine(newspaperManager.AnimateNewspaperIn());

            // 8. Wait for player to click to continue
            waitingForNewspaperClick = true;
            while (waitingForNewspaperClick)
            {
                if (Input.GetMouseButtonDown(0))
                    waitingForNewspaperClick = false;
                yield return null;
            }

            // 9. Move newspaper parent offscreen
            yield return StartCoroutine(newspaperManager.MoveParentOffscreen());
            
            // 10. Wait
            yield return new WaitForSeconds(0.25f);

            // 10.5. Shift background hue slightly for the next round
            StartCoroutine(LerpBackgroundHue());

            // 11. Repeat
            currentScenario = null;
            
            // Update header text with new month and approval rating
            turnNumber++;
            UpdateHeaderText();
            
            // Check for game over conditions
            if (approvalRating == 0 || approvalRating == 100)
            {
                TriggerGameOver();
                yield break;
            }
        }
    }

    private void InitializeBackgroundHue()
    {
        if (backgroundImage == null)
            return;

        backgroundImage.color = GetColorForApprovalRating();
    }

    private IEnumerator LerpBackgroundHue()
    {
        if (backgroundImage == null)
            yield break;

        Color startColor = backgroundImage.color;
        Color targetColor = GetColorForApprovalRating();

        float elapsed = 0f;
        while (elapsed < hueLerpDuration)
        {
            float t = hueLerpDuration > 0f ? elapsed / hueLerpDuration : 1f;
            backgroundImage.color = Color.Lerp(startColor, targetColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        backgroundImage.color = targetColor;
    }

    private Color GetColorForApprovalRating()
    {
        float approval01 = Mathf.Clamp01(approvalRating / 100f);
        return Color.Lerp(minApprovalColor, maxApprovalColor, approval01);
    }
    
    private void UpdateHeaderText()
    {
        // In the format "Month 1 - Approval Rating 50%"
        if (headerText != null)
        {
            headerText.text = $"Month {turnNumber} - Approval Rating {approvalRating}%";
        }

    }

    private void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        pawManager.SetLive(false);

        if (gameOverHeaderText != null)
        {
            gameOverHeaderText.text = $"Game Over - {turnNumber} Months In Office";
        }

        if (gameOverSubheaderText != null)
        {
            gameOverSubheaderText.text = GetGameOverSubheader();
        }

        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(true);
        }

        StartCoroutine(DrawApprovalGraph());
    }
    
    private string GetGameOverSubheader()
    {
        if(approvalRating == 100)
        {
            return "With your approval rating reaching unprecedented heights, you are constantly mobbed by adoring fans, making it impossible to perform the duties of your office.";
        }
        else
        {
            return "With your approval rating hitting rock bottom, you are hastily removed from office in disgrace. You return to a carefree life as a normal dog, free from the duties of office.";
        }
    }

    public void RestartGame()
    {
        StopAllCoroutines();

        if (pawManager != null)
        {
            pawManager.StopAllCoroutines();
            pawManager.OnButtonSlammed = null;
            pawManager.SetLive(false);
        }

        if (issueManager != null)
        {
            issueManager.StopAllCoroutines();
            issueManager.ResetIssueImmediate();
        }

        if (newspaperManager != null)
        {
            newspaperManager.StopAllCoroutines();
            newspaperManager.ResetNewspaper();

            if (newspaperManager.headlineText != null)
                newspaperManager.headlineText.text = "";

            if (newspaperManager.subheadingText != null)
                newspaperManager.subheadingText.text = "";

            if (newspaperManager.approvalRatingText != null)
                newspaperManager.approvalRatingText.text = "";
        }

        if (scenarioManager != null)
        {
            scenarioManager.ResetScenarioPool();
        }

        currentScenario = null;
        waitingForSlam = false;
        waitingForNewspaperClick = false;
        isGameOver = false;
        approvalRating = 50;
        turnNumber = 1;
        ResetApprovalHistory();
        ClearApprovalGraph();

        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(false);
        }

        InitializeBackgroundHue();
        UpdateHeaderText();

        StartCoroutine(GameplayLoop());
    }

    // Called when scenario changes
    void OnScenarioChanged(Scenario scenario)
    {
        currentScenario = scenario;
    }

    // Called when a response is played (optional, not used here)
    void OnResponsePlayed(Response response) { }

    private int GetApprovalDelta(ApprovalRatingEffect effect)
    {
        switch (effect)
        {
            case ApprovalRatingEffect.Mixed:
                int mixedDelta = 0;
                while (mixedDelta == 0)
                    mixedDelta = rng.Next(-9, 10); // -9 to +9, but not zero
                return mixedDelta;
            case ApprovalRatingEffect.PositiveSmall:
                return rng.Next(10, 25); // +10 to +24
            case ApprovalRatingEffect.PositiveLarge:
                return rng.Next(25, 50); // +25 to +49
            case ApprovalRatingEffect.NegativeSmall:
                return -rng.Next(10, 25); // -10 to -24
            case ApprovalRatingEffect.NegativeLarge:
                return -rng.Next(25, 50); // -25 to -49
            default:
                return 0;
        }
    }

    private string GetApprovalPhrase(ApprovalRatingEffect effect, int delta)
    {
        if (effect == ApprovalRatingEffect.Mixed)
        {
            if (delta > 0)
                return positiveSmallPhrases[rng.Next(positiveSmallPhrases.Length)];
            else
                return negativeSmallPhrases[rng.Next(negativeSmallPhrases.Length)];
        }
        else if (effect == ApprovalRatingEffect.PositiveSmall)
        {
            return positiveSmallPhrases[rng.Next(positiveSmallPhrases.Length)];
        }
        else if (effect == ApprovalRatingEffect.PositiveLarge)
        {
            return positiveLargePhrases[rng.Next(positiveLargePhrases.Length)];
        }
        else if (effect == ApprovalRatingEffect.NegativeSmall)
        {
            return negativeSmallPhrases[rng.Next(negativeSmallPhrases.Length)];
        }
        else if (effect == ApprovalRatingEffect.NegativeLarge)
        {
            return negativeLargePhrases[rng.Next(negativeLargePhrases.Length)];
        }
        else
        {
            return "Approval rating";
        }
    }

    private void ResetApprovalHistory()
    {
        approvalHistory.Clear();
        approvalHistory.Add(approvalRating);
    }

    private IEnumerator DrawApprovalGraph()
    {
        if (graphArea == null || approvalHistory.Count == 0)
            yield break;

        ClearApprovalGraph();

        graphLineObject = new GameObject("ApprovalGraphLine", typeof(LineRenderer));
        graphLineObject.transform.SetParent(graphArea, false);

        LineRenderer lineRenderer = graphLineObject.GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 1;
        lineRenderer.startWidth = graphLineWidth;
        lineRenderer.endWidth = graphLineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.sortingOrder = 1;
        lineRenderer.material = graphLineMaterial;

        // Pre-compute all positions
        Rect rect = graphArea.rect;
        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;
        int count = approvalHistory.Count;

        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float xT = count > 1 ? (float)i / (count - 1) : 0.5f;
            float yT = Mathf.Clamp01(approvalHistory[i] / 100f);
            positions[i] = new Vector3(Mathf.Lerp(xMin, xMax, xT), Mathf.Lerp(yMin, yMax, yT), 0f);
        }

        // Place the first point immediately
        lineRenderer.SetPosition(0, positions[0]);

        if (count == 1 || graphAnimDuration <= 0f)
        {
            lineRenderer.positionCount = count;
            for (int i = 0; i < count; i++)
                lineRenderer.SetPosition(i, positions[i]);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < graphAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / graphAnimDuration);
            float virtualIndex = t * (count - 1);
            int fullPoints = Mathf.FloorToInt(virtualIndex);
            float frac = virtualIndex - fullPoints;

            // positionCount = fully drawn points + 1 animated tip
            int newCount = Mathf.Min(fullPoints + 2, count);
            lineRenderer.positionCount = newCount;

            for (int i = 0; i <= fullPoints && i < count; i++)
                lineRenderer.SetPosition(i, positions[i]);

            // Interpolate the live tip between the last full point and the next
            if (fullPoints + 1 < count)
                lineRenderer.SetPosition(fullPoints + 1, Vector3.Lerp(positions[fullPoints], positions[fullPoints + 1], frac));

            yield return null;
        }

        // Snap to final state
        lineRenderer.positionCount = count;
        for (int i = 0; i < count; i++)
            lineRenderer.SetPosition(i, positions[i]);
    }

    private void ClearApprovalGraph()
    {
        if (graphLineObject != null)
        {
            Destroy(graphLineObject);
            graphLineObject = null;
        }
    }
}