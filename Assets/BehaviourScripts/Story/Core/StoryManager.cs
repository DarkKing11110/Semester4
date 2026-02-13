using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryStepData[] storySteps;

    int currentStepIndex = 0;
    StoryStep currentStep;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartStep(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentStep != null && currentStep.IsComplete)
        {
            currentStep.FinishStep();
            currentStepIndex++;

            if (currentStepIndex < storySteps.Length)
            {
                StartStep(currentStepIndex);
            }
            else
            {
                Debug.Log("Story complete!");
            }
        }
        
    }

    void StartStep(int index)
    {
        currentStep = new StoryStep(storySteps[index]);
        currentStep.StartStep();
    }
}
