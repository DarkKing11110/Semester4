using UnityEngine;

[CreateAssetMenu(menuName = "Story/StoryStepData")]
public class StoryStepData : ScriptableObject
{
    public string stepID; // Name des Story-Schritts
    public string description; // Beschreibung des Schritts

    public StoryConditionData[] condition;
}
