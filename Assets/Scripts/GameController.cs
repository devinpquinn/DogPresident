using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public PawManager pawManager;
    public ScenarioManager scenarioManager;
    public BriefingManager briefingManager;
    public NewspaperManager newspaperManager;

    private int scenariosPlayed = 0;
    private Scenario currentScenario;

    private bool waitingForSlam = false;
    private bool waitingForNewspaperClick = false;

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

        // Start the loop
        StartCoroutine(GameplayLoop());
    }

    IEnumerator GameplayLoop()
    {
        while (true)
        {
            // 1. Paw at rest, not live or tracking

            // 2. Select a scenario (advance)
            scenarioManager.AdvanceToNextScenario();

            // Wait for scenario to be loaded and event to fire
            yield return new WaitUntil(() => currentScenario != null);

            // 3. Slide in briefing, show scenario number
            scenariosPlayed++;
            briefingManager.scenarioNumberText.text = scenariosPlayed.ToString();
            briefingManager.promptText.text = ""; // Clear until open
            yield return StartCoroutine(briefingManagerSlideIn());

            // 4. Set prompt text
            briefingManager.promptText.text = currentScenario.promptText;
            
            // 4.5. Wait
            yield return new WaitForSeconds(0.5f);

            // 5. Set paw live and tracking
            pawManager.SetLive(true);

            // 6. Wait for player to slam a button (wait for slam and get index)
            int chosenResponse = -1;
            waitingForSlam = true;
            pawManager.OnButtonSlammed = (index) => { chosenResponse = index; waitingForSlam = false; };
            yield return new WaitUntil(() => !waitingForSlam);

            // Wait for the slam animation (including return) to finish
            yield return StartCoroutine(pawManager.WaitForSlamComplete());

            // 7. Set paw not live or tracking
            pawManager.SetLive(false);
            
            // 7.5. Wait
            yield return new WaitForSeconds(0.5f);

            // 8. Show newspaper with result
            Response response = currentScenario.responses[chosenResponse];

            // Apply approval effect
            int delta = GetApprovalDelta(response.approvalEffect);
            approvalRating = Mathf.Clamp(approvalRating + delta, 0, 100);

            string phrase = GetApprovalPhrase(response.approvalEffect, delta);
            newspaperManager.headlineText.text = response.headline;
            newspaperManager.subheadingText.text = response.subheading;
            newspaperManager.approvalRatingText.text = $"{phrase} to {approvalRating} percent";
            StartCoroutine(newspaperManager.AnimateNewspaperIn());

            // 9. Wait for player to click to continue
            waitingForNewspaperClick = true;
            while (waitingForNewspaperClick)
            {
                if (Input.GetMouseButtonDown(0))
                    waitingForNewspaperClick = false;
                yield return null;
            }

            // 10. Move newspaper parent offscreen
            yield return StartCoroutine(newspaperManager.MoveParentOffscreen());
            
            // 10.5. Wait
            yield return new WaitForSeconds(0.5f);

            // 11. Repeat
            currentScenario = null;
        }
    }

    // Helper to wait for briefing slide in
    IEnumerator briefingManagerSlideIn()
    {
        // Simulate pressing space to slide in
        briefingManager.StartCoroutine("SlideIn");
        // Wait for folder to open (wait for the open GameObject to be active)
        yield return new WaitUntil(() => briefingManager.folderOpen.activeSelf);
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