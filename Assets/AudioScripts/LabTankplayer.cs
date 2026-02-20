using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class LabTankplayer : MonoBehaviour
{
	[Header("FMOD Event")]
	[Tooltip("FMOD Studio Event to play for the lab tank.")]
	[SerializeField] private EventReference labEvent;

	[Header("3D Event Settings")]
	[SerializeField] private bool use3D = true;
	[SerializeField] private bool attachToGameObject = true;
	[SerializeField] private Rigidbody attachRigidbody;
	[SerializeField] private Vector3 positionOffset = Vector3.zero;

	[Header("Playback Attributes")]
	[Range(0f, 1f)]
	[SerializeField] private float volume = 1f;
	[Range(0.5f, 2f)]
	[SerializeField] private float pitch = 1f;
	[SerializeField] private bool allowFadeOutOnStop = false;

	[Header("Occlusion Integration")]
	[Tooltip("Enable occlusion and parameter updates using an AudioOcclusion source.")]
	[SerializeField] private bool occlusionEnabled = false;
	[Tooltip("Optional AudioOcclusion component to use for occlusion values.")]
	[SerializeField] private AudioOcclusion occlusionSource;
	[Tooltip("Use parameter names/values from the occlusion source. If false, use the custom fields below.")]
	[SerializeField] private bool useOcclusionSourceSettings = true;
	[SerializeField] private string occlusionParamName;
	[SerializeField] private string occlusionFadeParamName;
	[SerializeField] private string materialParamName;
	[SerializeField] private int materialParamValue = 0;

	private EventInstance currentInstance;
	[Header("Playback Options")]
	[Tooltip("Automatically play the FMOD event on Start.")]
	[SerializeField] private bool playOnStart = true;

	private void Start()
	{
		if (playOnStart)
			PlayEvent();
	}

	private void PlayEvent()
	{
		StopCurrentEventIfPlaying();

		if (!labEvent.IsNull)
		{
			currentInstance = RuntimeManager.CreateInstance(labEvent);

			if (use3D)
			{
				if (attachToGameObject)
				{
					RuntimeManager.AttachInstanceToGameObject(currentInstance, transform, attachRigidbody);
				}
				else
				{
					var attrs = RuntimeUtils.To3DAttributes(transform);
					attrs.position.x += positionOffset.x;
					attrs.position.y += positionOffset.y;
					attrs.position.z += positionOffset.z;
					currentInstance.set3DAttributes(attrs);
				}
			}

			if (volume != 1f)
				currentInstance.setVolume(volume);
			if (pitch != 1f)
				currentInstance.setPitch(pitch);

			if (occlusionEnabled && occlusionSource != null)
			{
				string matParam = useOcclusionSourceSettings ? occlusionSource.MaterialParamName : materialParamName;
				int matVal = useOcclusionSourceSettings ? occlusionSource.MaterialParamValue : materialParamValue;
				if (!string.IsNullOrEmpty(matParam))
				{
					currentInstance.setParameterByName(matParam, matVal);
				}
			}

			currentInstance.start();
		}
	}

	private void StopCurrentEventIfPlaying()
	{
		if (currentInstance.isValid())
		{
			currentInstance.getPlaybackState(out PLAYBACK_STATE state);
			if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING)
			{
				currentInstance.stop(allowFadeOutOnStop ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			currentInstance.release();
			currentInstance.clearHandle();
		}
	}

	private void Update()
	{
		if (occlusionEnabled && occlusionSource != null && currentInstance.isValid())
		{
			if (occlusionSource.TryComputeOcclusion(transform, out float fadeValue, out bool obstructed))
			{
				string fadeParam = useOcclusionSourceSettings ? occlusionSource.OcclusionFadeParamName : occlusionFadeParamName;
				string occParam = useOcclusionSourceSettings ? occlusionSource.OcclusionParamName : occlusionParamName;
				if (!string.IsNullOrEmpty(fadeParam))
				{
					currentInstance.setParameterByName(fadeParam, fadeValue);
				}
				if (!string.IsNullOrEmpty(occParam))
				{
					currentInstance.setParameterByName(occParam, obstructed ? 1f : 0f);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayEvent();
	}

	private void OnTriggerExit(Collider other)
	{
		StopCurrentEventIfPlaying();
	}

	private void OnDestroy()
	{
		if (currentInstance.isValid())
		{
			currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			currentInstance.release();
		}
	}
}

