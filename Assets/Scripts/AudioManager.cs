using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float MasterVolume = 1;
    [Range(0, 1)]
    public float MusicVolume = 1;
    [Range(0, 1)]
    public float AmbienceVolume = 1;
    [Range(0, 1)]
    public float DialogueVolume = 1;

    private Bus MasterBus;
    private Bus MusicBus;
    private Bus AmbienceBus;
    private Bus Dialoguebus;

    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        MasterBus = RuntimeManager.GetBus("bus:/");
        MusicBus = RuntimeManager.GetBus("bus:/");
        AmbienceBus = RuntimeManager.GetBus("bus:/");
        Dialoguebus = RuntimeManager.GetBus("bus:/");
    }

    private void Update()
    {
        MasterBus.setVolume(MasterVolume);
        MusicBus.setVolume(MusicVolume);
        AmbienceBus.setVolume(AmbienceVolume);
        Dialoguebus.setVolume(DialogueVolume);
    }
}
