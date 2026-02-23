using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class AudioSlider : MonoBehaviour
{
    private enum VolumeType
    {
        MASTER,
        SFX,
        VOPLAYER,
        AMBIENCE,
        ENVIORNMENT,
        UI
    }

    [SerializeField] private VolumeType volumeType;

    private Slider VolumeSlider;

    private void Awake()
    {
        VolumeSlider = this.GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        switch (volumeType)
        { 
            case VolumeType.MASTER:
                VolumeSlider.value = AudioManager.instance.MasterVolume;
                break;
            case VolumeType.SFX:
                VolumeSlider.value = AudioManager.instance.SFXVolume;
                break;
            case VolumeType.VOPLAYER:
                VolumeSlider.value = AudioManager.instance.VOVolume;
                break;
            case VolumeType.AMBIENCE:
                VolumeSlider.value = AudioManager.instance.AmbienceVolume;
                break;
            case VolumeType.ENVIORNMENT:
                VolumeSlider.value = AudioManager.instance.EnvironmentVolume;
                break;
            case VolumeType.UI:
                VolumeSlider.value = AudioManager.instance.UIVolume;
                break;
            default:
                Debug.LogWarning("Volume type not supported: " + volumeType);
                break;
        }
    }

    public void OnSliderValueChange()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                AudioManager.instance.MasterVolume = VolumeSlider.value;
                break;
            case VolumeType.SFX:
                AudioManager.instance.SFXVolume = VolumeSlider.value;
                break;
            case VolumeType.VOPLAYER:
                AudioManager.instance.VOVolume = VolumeSlider.value;
                break;
            case VolumeType.AMBIENCE:
                AudioManager.instance.AmbienceVolume = VolumeSlider.value;
                break;
            case VolumeType.ENVIORNMENT:
                AudioManager.instance.EnvironmentVolume = VolumeSlider.value; break;
            case VolumeType.UI:
                AudioManager.instance.UIVolume = VolumeSlider.value; break;
            default:
                Debug.LogWarning("Volume type not supported: " + volumeType);
                break;
        }
    }
}
