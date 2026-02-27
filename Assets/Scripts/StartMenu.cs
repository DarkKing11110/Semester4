using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StartMenu : MonoBehaviour
{
    [SerializeField] GameObject StartPanel;
    [SerializeField] GameObject SettingsPanel;
    [SerializeField] GameObject CreditsPanel;
    [SerializeField] TMP_Text Nest;
    private string color = "Black";

    private void Start()
    {
        StartPanel.SetActive(true);
        Nest.text = ("<color=" + color + ">N.E.S.T</color>"  );
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1); //Number to be changed
        StartPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackButton()
    {
        StartPanel.SetActive(true);
        SettingsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
    }

    public void Settings()
    {
        StartPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void CreditsMenu()
    {
        StartPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {

    }
    public void HoverSound()
    {
        RuntimeManager.PlayOneShot("event:/UI/Hover SFX");
    }
    public void ClickSound()
    {
        RuntimeManager.PlayOneShot("event:/UI/Select SFX");
    }
}

