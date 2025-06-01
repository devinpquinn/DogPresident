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

            // 8. Show newspaper with result
            Response response = currentScenario.responses[chosenResponse];
            newspaperManager.headlineText.text = response.headline;
            newspaperManager.subheadingText.text = response.subheading;
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
}