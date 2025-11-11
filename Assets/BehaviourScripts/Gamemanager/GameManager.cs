using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { MainMenu, Playing, Won, Dead }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private bool showEndScreen = false;
    private string endMessage = "";

    void Awake()
    {
        // Singleton-Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Starte im Hauptmenü
        CurrentState = GameState.MainMenu;
        showEndScreen = false;
    }

    public void StartGame()
    {
        showEndScreen = false;
        endMessage = "";
        CurrentState = GameState.Playing;

        // Szene laden und Spielersteuerung nach dem Laden aktivieren
        SceneManager.LoadScene("HH_Demo");
        StartCoroutine(EnablePlayerControlNextFrame());
    }

    public void QuitGame()
    {
        Debug.Log("Spiel wird beendet.");
        Application.Quit();
    }

    private System.Collections.IEnumerator EnablePlayerControlNextFrame()
    {
        // Warten bis die Szene geladen ist
        yield return null;
        EnablePlayerControl(true);
    }


    public void PlayerDied()
    {
        CurrentState = GameState.Dead;
        endMessage = "Du bist gestorben!";
        showEndScreen = true;
        EnablePlayerControl(false);
        Invoke(nameof(ReturnToMenu), 2.5f); // 2.5 Sekunden warten
    }

    public void PlayerWon()
    {
        CurrentState = GameState.Won;
        endMessage = "Du hast gewonnen!";
        showEndScreen = true;
        EnablePlayerControl(false);
        Invoke(nameof(ReturnToMenu), 2.5f); // 2.5 Sekunden warten
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        CurrentState = GameState.MainMenu;
        showEndScreen = false;
        endMessage = "";
        EnablePlayerControl(false);
    }

    private void EnablePlayerControl(bool enable)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var behaviour = player.GetComponent<PlayerBehaviour>();
            if (behaviour != null)
                behaviour.enabled = enable;
        }
    }
}
