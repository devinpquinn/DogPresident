using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine.SceneManagement;

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
    public Button restartButton;
    private int turnNumber = 1;

    private Scenario currentScenario;

    private bool waitingForSlam = false;
    private bool waitingForNewspaperClick = false;
    private bool isGameOver = false;

    private int approvalRating = 50; // Start at 50%

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

            if (approvalRating == 0 || approvalRating == 100)
            {
                TriggerGameOver();
                yield break;
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

        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
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
}