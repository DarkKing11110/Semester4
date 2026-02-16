using UnityEngine;
using FMODUnity;

public class ReverbManager : MonoBehaviour
{
    public static ReverbManager Instance;

    float currentValue;
    float targetValue;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        currentValue = Mathf.Lerp(
            currentValue,
            targetValue,
            Time.deltaTime * 3f
        );

        RuntimeManager.StudioSystem
            .setParameterByName("Environment", currentValue);
    }

    public void SetEnvironment(float value)
    {
        targetValue = value;
    }
}

