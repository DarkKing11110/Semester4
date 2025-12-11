using UnityEngine;
using FMODUnity;

public class Paramaeterchanger : MonoBehaviour
{
    [Header("FMOD Parameter Settings")]
    [SerializeField] private string parameterName = "Activate Generator add on"; // FMOD parameter name
    [SerializeField] [Range(0f, 1f)] private float baseAmbienceValue = 0.0f;     // 0.0 - 0.49
    [SerializeField] [Range(0f, 1f)] private float generatorAmbienceValue = 1.0f; // 0.49 - 1.0

    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player"; // tag used to identify player

    void Start()
    {
        // Set default to Base ambience at startup
        RuntimeManager.StudioSystem.setParameterByName(parameterName, baseAmbienceValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        RuntimeManager.StudioSystem.setParameterByName(parameterName, generatorAmbienceValue);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        RuntimeManager.StudioSystem.setParameterByName(parameterName, baseAmbienceValue);
    }
}
