using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingButtons : MonoBehaviour
{
    private float timer = 0;
    public GameObject PlayagainButton;
    public GameObject QuitButton;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 40)
        {
            PlayagainButton.SetActive(true);
            QuitButton.SetActive(true);
        }
    }
    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
}
