using UnityEngine;
using System;

public class StoryManager : MonoBehaviour
{
    public StoryStepData[] storySteps;

    // Aktueller Step-Index öffentlich lesbar
    public int CurrentStepIndex => currentStepIndex;
   
    // Aktuelle Step-Daten (ScriptableObject) öffentlich lesbar
    public StoryStepData CurrentStepData => 
        (currentStepIndex >= 0 && currentStepIndex < storySteps.Length) ? storySteps[currentStepIndex] : null;

    // Aktueller Step (Runtime) öffentlich lesbar
    public StoryStep CurrentStep => currentStep;

    // Event: Benachrichtigung bei Schrittwechsel
    public event Action<int, StoryStepData> OnStepStarted;

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
        Debug.Log("Step " + currentStepIndex + " complete: " + storySteps[currentStepIndex].name);
        if (currentStep != null && currentStep.IsComplete)
        {
            currentStep.FinishStep();
            Debug.Log("Step " + currentStepIndex + " complete: " + storySteps[currentStepIndex].name);
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
        OnStepStarted?.Invoke(index, storySteps[index]);
    }

    // Hilfsmethode: Name/ID der aktuellen Mission holen (falls im StoryStepData vorhanden)
    public string GetCurrentMissionIdOrName()
    {
        var data = CurrentStepData;
        if (data == null) return null;

        // Falls StoryStepData z. B. ein Feld 'stepName' oder 'id' hat, hier bevorzugt zurückgeben.
        // Passe die folgende Zeile an deine Felder im StoryStepData an:
        return data.name; // ScriptableObject-Name als Fallback
    }

    public string GetCurrentId()
    {
        var data = CurrentStepData;
        if (data == null) return null;

        // Falls StoryStepData z. B. ein Feld 'stepName' oder 'id' hat, hier bevorzugt zurückgeben.
        // Passe die folgende Zeile an deine Felder im StoryStepData an:
        return data.stepID; // ScriptableObject-Name als Fallback
    }
}
