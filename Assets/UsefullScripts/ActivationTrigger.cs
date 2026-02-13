using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class ActivationTrigger : MonoBehaviour
{
    [SerializeField] string activationTag;
    [SerializeField] UnityEvent activationMethods;
    [SerializeField] UnityEvent deactivationMethods;
    [Header("FMOD Events")]
    [Tooltip("FMOD Studio Event für Tür-Öffnen.")]
    [SerializeField] private EventReference doorOpenEvent;
    [Tooltip("FMOD Studio Event für Tür-Schließen.")]
    [SerializeField] private EventReference doorCloseEvent;

    [Header("3D Event Settings")]
    [Tooltip("Enable 3D spatialization for events.")]
    [SerializeField] private bool use3D = true;
    [Tooltip("Attach event instance to this Transform to auto-update position.")]
    [SerializeField] private bool attachToGameObject = true;
    [Tooltip("Optional Rigidbody used when attaching for velocity updates.")]
    [SerializeField] private Rigidbody attachRigidbody;
    [Tooltip("Position offset applied when not attaching.")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    [Header("Playback Attributes")]
    [Tooltip("Initial volume for the event instance (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [Tooltip("Pitch multiplier for the event instance (0.5-2.0). 1 = normal.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float pitch = 1f;
    [Tooltip("Use fadeout when stopping the previous event.")]
    [SerializeField] private bool allowFadeOutOnStop = false;

    [Header("Occlusion Settings")]
    [Tooltip("Enable occlusion checks via AudioOcclusion.")]
    [SerializeField] private bool occlusionEnabled = false;
    [Tooltip("AudioOcclusion source to use for computations.")]
    [SerializeField] private AudioOcclusion occlusionSource;
    [Tooltip("Use parameter names from AudioOcclusion. If false, use custom names below.")]
    [SerializeField] private bool useOcclusionSourceSettings = true;
    [Tooltip("FMOD parameter name for occlusion (0 or 1). Only used if not using source settings.")]
    [SerializeField] private string occlusionParamName;
    [Tooltip("FMOD parameter name for occlusion fade value (0-1). Only used if not using source settings.")]
    [SerializeField] private string occlusionFadeParamName;
    [Tooltip("Material parameter name if needed (e.g., reverb type). Only used if not using source settings.")]
    [SerializeField] private string materialParamName;
    [Tooltip("Material parameter value to set on start when not using source settings.")]
    [SerializeField] private int materialParamValue = 0;

    private EventInstance currentEventInstance;

    private void PlayEvent(EventReference eventRef)
    {
        StopCurrentEventIfPlaying();
        currentEventInstance = RuntimeManager.CreateInstance(eventRef);

        if (use3D)
        {
            if (attachToGameObject)
            {
                RuntimeManager.AttachInstanceToGameObject(currentEventInstance, transform, attachRigidbody);
            }
            else
            {
                var attrs = RuntimeUtils.To3DAttributes(transform);
                attrs.position.x += positionOffset.x;
                attrs.position.y += positionOffset.y;
                attrs.position.z += positionOffset.z;
                currentEventInstance.set3DAttributes(attrs);
            }
        }

        if (volume != 1f)
        {
            currentEventInstance.setVolume(volume);
        }
        if (pitch != 1f)
        {
            currentEventInstance.setPitch(pitch);
        }

        if (occlusionEnabled && occlusionSource != null)
        {
            string matParam = useOcclusionSourceSettings ? occlusionSource.MaterialParamName : materialParamName;
            int matValue = useOcclusionSourceSettings ? occlusionSource.MaterialParamValue : materialParamValue;
            if (!string.IsNullOrEmpty(matParam))
            {
                currentEventInstance.setParameterByName(matParam, matValue);
            }
        }

        currentEventInstance.start();
    }

    private void StopCurrentEventIfPlaying()
    {
        if (currentEventInstance.isValid())
        {
            currentEventInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING)
            {
                currentEventInstance.stop(allowFadeOutOnStop ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            currentEventInstance.release();
            currentEventInstance.clearHandle();
        }
    }

    private void Update()
    {
        if (occlusionEnabled && occlusionSource != null && currentEventInstance.isValid())
        {
            if (occlusionSource.TryComputeOcclusion(transform, out float fadeValue, out bool obstructed))
            {
                string fadeParam = useOcclusionSourceSettings ? occlusionSource.OcclusionFadeParamName : occlusionFadeParamName;
                string occParam = useOcclusionSourceSettings ? occlusionSource.OcclusionParamName : occlusionParamName;
                if (!string.IsNullOrEmpty(fadeParam))
                {
                    currentEventInstance.setParameterByName(fadeParam, fadeValue);
                }
                if (!string.IsNullOrEmpty(occParam))
                {
                    currentEventInstance.setParameterByName(occParam, obstructed ? 1f : 0f);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag(activationTag))
        //{
            
        //}
        activationMethods?.Invoke();
        PlayEvent(doorOpenEvent);

    }

    private void OnTriggerExit(Collider other)
    {
        deactivationMethods?.Invoke();
        PlayEvent(doorCloseEvent);
    }

    private void OnDestroy()
    {
        if (currentEventInstance.isValid())
        {
            currentEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentEventInstance.release();
        }
    }
}
