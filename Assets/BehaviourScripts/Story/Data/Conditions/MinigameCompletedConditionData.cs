using UnityEngine;


[CreateAssetMenu(menuName = "Story/Conditions/Minigame Completed")]
public class MinigameCompletedConditionData : StoryConditionData
{

    public string minigameID;
    public override StoryCondition CreateCondition()
    {
        return new MinigameCompletedCondition(this);
    }
}
