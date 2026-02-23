using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float MasterVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;
    [Range(0, 1)]
    public float VOVolume = 1;
    [Range(0, 1)]
    public float AmbienceVolume = 1;
    [Range(0, 1)]
    public float EnvironmentVolume = 1;
    [Range(0, 1)]
    public float UIVolume = 1;

    private Bus MasterBus;
    private Bus SFXBus;
    private Bus VOBus;
    private Bus AmbienceBus;
    private Bus EnvironmentBus;
    private Bus UIBus;

    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        MasterBus = RuntimeManager.GetBus("bus:/");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        VOBus = RuntimeManager.GetBus("bus:/VO-Player");
        AmbienceBus = RuntimeManager.GetBus("bus:/Ambience");
        EnvironmentBus = RuntimeManager.GetBus("bus:/Environment");
        UIBus = RuntimeManager.GetBus("bus:/UI");
    }

    private void Update()
    {
        MasterBus.setVolume(MasterVolume);
        SFXBus.setVolume(SFXVolume);
        VOBus.setVolume(VOVolume);
        AmbienceBus.setVolume(AmbienceVolume);
        EnvironmentBus.setVolume(EnvironmentVolume);
        UIBus.setVolume(UIVolume);
    }
}
