using UnityEngine;


public class MinigameCompletedCondition : StoryCondition
{
    MinigameCompletedConditionData data;

    public MinigameCompletedCondition(MinigameCompletedConditionData data)
    {
        this.data = data;
    }

    public override void initialize()
    {
        GameEvents.OnMinigameCompleted += onMinigameCompleted;
    }

    public override void Dispose()
    {
       GameEvents.OnMinigameCompleted -= onMinigameCompleted;
    }

    void onMinigameCompleted(string id)
    {
        if (id == data.minigameID)
        {
            IsTrue = true;
        }
    }
}
