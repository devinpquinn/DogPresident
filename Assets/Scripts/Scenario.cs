using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Scenario", menuName = "Presidential/Scenario", order = 0)]
public class Scenario : ScriptableObject
{
    [TextArea]
    public string promptText;

    public List<string> responses = new List<string>();
}
