using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class AudioSlider : MonoBehaviour
{
    private enum VolumeType
    {
        MASTER,
        MUSIC,
        AMBIENCE,
        DIALOGUE
    }

    [SerializeField] private VolumeType volumeType;

    private Slider VolumeSlider;

    private void Awake()
    {
        VolumeSlider = this.GetComponent<Slider>();
    }

    private void Update()
    {
        switch (volumeType)
        { 
            case VolumeType.MASTER:
                VolumeSlider.value = AudioManager.instance.MasterVolume;
                break;
            case VolumeType.MUSIC:
                VolumeSlider.value = AudioManager.instance.MusicVolume;
                break;
            case VolumeType.AMBIENCE:
                VolumeSlider.value = AudioManager.instance.AmbienceVolume;
                break;
            case VolumeType.DIALOGUE:
                VolumeSlider.value = AudioManager.instance.DialogueVolume;
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
            case VolumeType.MUSIC:
                AudioManager.instance.MusicVolume = VolumeSlider.value;
                break;
            case VolumeType.AMBIENCE:
                AudioManager.instance.AmbienceVolume = VolumeSlider.value;
                break;
            case VolumeType.DIALOGUE:
                AudioManager.instance.DialogueVolume = VolumeSlider.value;
                break;
            default:
                Debug.LogWarning("Volume type not supported: " + volumeType);
                break;
        }
    }
}
