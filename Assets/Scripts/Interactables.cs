using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class Interactables : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject Panel;
    [Header("FMOD Events")]
    [Tooltip("Event played when the terminal opens.")]
    [SerializeField] private EventReference terminalOpenEvent;
    [Tooltip("Event played when the terminal closes.")]
    [SerializeField] private EventReference terminalCloseEvent;
    [Header("FMOD Settings")]
    [SerializeField] private bool allowFadeOutOnStop = true;

    private EventInstance _currentEventInstance;


    private void Start()
    {
        Panel.SetActive(false);
    }
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKey(KeyCode.E) && Panel != null && !Panel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            print("Works");
            Panel.SetActive(true);
            PlayEvent(terminalOpenEvent);
        }
        if (Input.GetKey(KeyCode.Escape) && Panel != null && Panel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Panel.SetActive(false);
            PlayEvent(terminalCloseEvent);
        }
    }

    private void PlayEvent(EventReference eventRef)
    {
        if (eventRef.IsNull) return;

        StopCurrentEventIfPlaying();
        _currentEventInstance = RuntimeManager.CreateInstance(eventRef);
        RuntimeManager.AttachInstanceToGameObject(_currentEventInstance, transform);
        _currentEventInstance.start();
    }

    private void StopCurrentEventIfPlaying()
    {
        if (_currentEventInstance.isValid())
        {
            _currentEventInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING)
            {
                _currentEventInstance.stop(allowFadeOutOnStop ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            _currentEventInstance.release();
            _currentEventInstance.clearHandle();
        }
    }

    private void OnDestroy()
    {
        if (_currentEventInstance.isValid())
        {
            _currentEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _currentEventInstance.release();
        }
    }
}