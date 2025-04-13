using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewScenario", menuName = "DogPresident/Scenario", order = 0)]
public class Scenario : ScriptableObject
{
    [TextArea]
    public string promptText;

    public List<Response> responses = new List<Response>();
}

[System.Serializable]
public class Response
{
    [TextArea]
    public string responseText;

    public ApprovalRatingEffect approvalEffect = ApprovalRatingEffect.NA;

    public string headline;
}

public enum ApprovalRatingEffect
{
    NA,
    PositiveLarge,
    PositiveSmall,
    NegativeSmall,
    NegativeLarge
}
