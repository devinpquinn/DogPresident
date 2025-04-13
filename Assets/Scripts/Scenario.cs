using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

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

    [ShowIf("HasApprovalEffect")]
    public string headline;

    // This method is used by NaughtyAttributes to determine whether to show the headline
    public bool HasApprovalEffect()
    {
        return approvalEffect != ApprovalRatingEffect.NA;
    }
}

public enum ApprovalRatingEffect
{
    NA,
    PositiveLarge,
    PositiveSmall,
    NegativeSmall,
    NegativeLarge
}
