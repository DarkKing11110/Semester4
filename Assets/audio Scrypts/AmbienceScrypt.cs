using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AmbienceScrypt : MonoBehaviour
{
    [Header("FMOD Parameter")]
    [SerializeField] private string parameterName = "Activate Generator add on";
    [SerializeField, Range(0.51f, 1.0f)] private float targetValue = 1.0f;

    [Header("Target Event (optional)")]
    [Tooltip("If set, the parameter will be applied to this emitter's EventInstance. Otherwise it will set a global parameter.")]
    [SerializeField] private bool useEventEmitter = false;
    [SerializeField] private StudioEventEmitter emitter; // optional

    [Header("Trigger Filter")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        // Try to auto-assign an emitter on the same GameObject
        if (emitter == null) emitter = GetComponent<StudioEventEmitter>();
    }

    private void OnValidate()
    {
        // Ensure value stays in requested band
        targetValue = Mathf.Clamp(targetValue, 0.51f, 1.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

        if (useEventEmitter && emitter != null)
        {
            EventInstance instance = emitter.EventInstance;
            if (instance.isValid())
            {
                instance.setParameterByName(parameterName, targetValue);
                return;
            }

            Debug.LogWarning($"AmbienceScrypt: Emitter instance not valid on {name}. Falling back to global parameter.");
        }

        RuntimeManager.StudioSystem.setParameterByName(parameterName, targetValue);
    }
}
