using UnityEngine;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    private List<Scenario> scenarioPool = new List<Scenario>();
    private int currentScenarioIndex = -1;
    private Scenario currentScenario;

    public delegate void OnScenarioChanged(Scenario newScenario);
    public event OnScenarioChanged onScenarioChanged;

    public delegate void OnResponsePlayed(Response response);
    public event OnResponsePlayed onResponsePlayed;

    void Start()
    {
        LoadAndShuffleScenarios();
        AdvanceToNextScenario();
    }

    private void LoadAndShuffleScenarios()
    {
        Scenario[] loadedScenarios = Resources.LoadAll<Scenario>("Scenarios"); // "Resources/Scenarios"
        scenarioPool = new List<Scenario>(loadedScenarios);
        ShuffleScenarioPool();
    }

    private void ShuffleScenarioPool()
    {
        for (int i = 0; i < scenarioPool.Count; i++)
        {
            Scenario temp = scenarioPool[i];
            int randomIndex = Random.Range(i, scenarioPool.Count);
            scenarioPool[i] = scenarioPool[randomIndex];
            scenarioPool[randomIndex] = temp;
        }

        currentScenarioIndex = -1;
    }

    public void AdvanceToNextScenario()
    {
        currentScenarioIndex++;

        if (currentScenarioIndex >= scenarioPool.Count)
        {
            ShuffleScenarioPool();
            currentScenarioIndex = 0;
        }

        currentScenario = scenarioPool[currentScenarioIndex];
        onScenarioChanged?.Invoke(currentScenario);
    }

    public void PlayResponse(int responseIndex)
    {
        if (currentScenario == null || responseIndex < 0 || responseIndex >= currentScenario.responses.Count)
        {
            Debug.LogWarning("Invalid response index or no current scenario.");
            return;
        }

        Response response = currentScenario.responses[responseIndex];
        Debug.Log("Player selected response: " + response.headline);
        Debug.Log("Subheading: " + response.subheading);

        onResponsePlayed?.Invoke(response);

        Debug.Log("Approval Effect: " + response.approvalEffect);
        AdvanceToNextScenario();
    }
}
