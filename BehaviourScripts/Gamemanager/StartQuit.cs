using UnityEngine;

public class StartQuit : MonoBehaviour
{
    // Für UI-Button: Diese Methoden im Button-Inspector zuweisen
    public void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
