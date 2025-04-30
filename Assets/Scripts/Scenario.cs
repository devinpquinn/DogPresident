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
    public string headline;
    public string subheading;

    public ApprovalRatingEffect approvalEffect = ApprovalRatingEffect.Mixed;
}

public enum ApprovalRatingEffect
{
    Mixed,
    PositiveLarge,
    PositiveSmall,
    NegativeSmall,
    NegativeLarge
}
