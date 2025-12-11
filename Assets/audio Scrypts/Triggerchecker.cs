using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Collider))]
public class Triggerchecker : MonoBehaviour
{
    [Header("FMOD Parameter")]
    [SerializeField] private string parameterName = "Activate Generator add on";
    [SerializeField, Range(0f, 1f)] private float insideValue = 1.0f;
    [SerializeField, Range(0f, 1f)] private float outsideValue = 0.0f;
    [SerializeField] private bool applyOnExit = true; // revert when player leaves

    [Header("Target Event (optional)")]
    [Tooltip("If enabled, apply the parameter on this emitter's EventInstance; otherwise set a global parameter.")]
    [SerializeField] private bool useEventEmitter = false;
    [SerializeField] private StudioEventEmitter emitter; // optional

    [Header("Trigger Filter")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debugging")]
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        if (emitter == null) emitter = GetComponent<StudioEventEmitter>();
    }

    private void OnValidate()
    {
        insideValue = Mathf.Clamp01(insideValue);
        outsideValue = Mathf.Clamp01(outsideValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        if (debugLogs) Debug.Log($"Triggerchecker: OnTriggerEnter by {other.name}, setting {parameterName}={insideValue}");
        SetParameter(insideValue);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!applyOnExit) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        if (debugLogs) Debug.Log($"Triggerchecker: OnTriggerExit by {other.name}, setting {parameterName}={outsideValue}");
        SetParameter(outsideValue);
    }

    private void SetParameter(float value)
    {
        if (useEventEmitter && emitter != null)
        {
            EventInstance instance = emitter.EventInstance;
            if (!instance.isValid())
            {
                if (debugLogs) Debug.Log($"Triggerchecker: EventInstance not valid on {name}. Attempting to Play() emitter and retry.");
                emitter.Play();
                instance = emitter.EventInstance;
            }

            if (instance.isValid())
            {
                instance.setParameterByName(parameterName, value);
                if (debugLogs) Debug.Log($"Triggerchecker: Set instance parameter {parameterName}={value} on {name}");
                return;
            }

            Debug.LogWarning($"Triggerchecker: Emitter instance still not valid on {name}. Falling back to global parameter.");
        }
        else if (useEventEmitter && emitter == null)
        {
            Debug.LogWarning($"Triggerchecker: useEventEmitter is true but no StudioEventEmitter assigned on {name}. Falling back to global parameter.");
        }

        RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        if (debugLogs) Debug.Log($"Triggerchecker: Set global parameter {parameterName}={value}");
    }
}
