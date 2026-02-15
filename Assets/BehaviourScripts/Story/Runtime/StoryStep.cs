using System.Collections.Generic;
using System.Linq;


public class StoryStep
{
    public StoryStepData Data { get; private set; }
    public List<StoryCondition> Conditions { get; private set; }

    public bool IsComplete => Conditions.All(c => c.IsTrue);

    public StoryStep(StoryStepData data)
    {
        Data = data;
        Conditions = new List<StoryCondition>();

        foreach (var conditionData in data.condition)
        {
            Conditions.Add(conditionData.CreateCondition());
        }
    }

    public void StartStep()
    {
        foreach (var condition in Conditions)
        {
            condition.initialize();
        }
    }

    public void FinishStep()
    {
        foreach (var condition in Conditions)
        {
            condition.Dispose();
        }
    }


}
