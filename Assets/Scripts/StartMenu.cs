using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StartMenu : MonoBehaviour
{
    [SerializeField] GameObject StartPanel;
    [SerializeField] GameObject SettingsPanel;
    [SerializeField] GameObject CreditsPanel;

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
}
