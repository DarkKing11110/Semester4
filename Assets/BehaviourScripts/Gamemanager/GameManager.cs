using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { MainMenu, Playing, Won, Dead }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("UI")]
    [Tooltip("Panel, das beim Tod angezeigt wird (Zwischenmenü mit 'Weiter' / 'Zum Hauptmenü').")]
    public GameObject deathMenuPanel;

    [Tooltip("Panel für den Win-/Credits-Screen.")]
    public GameObject winScreenPanel;

    [Header("Respawn")]
    [Tooltip("Respawn-Punkt für den Spieler. Wenn leer, wird die aktuelle Spielerposition beibehalten.")]
    public Transform respawnPoint;

    private bool showEndScreen = false;
    private string endMessage = "";

    void Awake()
    {
        // Singleton-Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // WICHTIG
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Panels in der neu geladenen Szene suchen (falls vorhanden)
        if (deathMenuPanel == null)
        {
            var deathObj = GameObject.FindWithTag("DeathMenuPanel");
            if (deathObj != null)
            {
                deathMenuPanel = deathObj;
                deathMenuPanel.SetActive(false);
            }
        }

        if (winScreenPanel == null)
        {
            var winObj = GameObject.FindWithTag("WinScreenPanel");
            if (winObj != null)
            {
                winScreenPanel = winObj;
                winScreenPanel.SetActive(false);
            }
        }

        if(respawnPoint == null)
            {
            var respawnObj = GameObject.FindWithTag("RespawnPoint");
            if (respawnObj != null)
                respawnPoint = respawnObj.transform;
        }
    }

    void Start()
    {
        // Starte im Hauptmenü
        CurrentState = GameState.MainMenu;
        showEndScreen = false;

        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);
        if (winScreenPanel != null)
            winScreenPanel.SetActive(false);
    }

    public void StartGame()
    {
        showEndScreen = false;
        endMessage = "";
        CurrentState = GameState.Playing;

        // Szene laden und Spielersteuerung nach dem Laden aktivieren
        SceneManager.LoadScene("Spaceship_enviorment");
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
        LockCursor(true);
    }

    public void PlayerDied()
    {
        CurrentState = GameState.Dead;
        endMessage = "Du bist gestorben!";
        showEndScreen = true;

        EnablePlayerControl(false);
        LockCursor(false);
        ShowWinScreen(false);
        ShowDeathMenu(true);
    }

    public void PlayerWon()
    {
        CurrentState = GameState.Won;
        endMessage = "Du hast gewonnen!";
        showEndScreen = true;

        EnablePlayerControl(false);
        LockCursor(false);
        ShowDeathMenu(false);
        ShowWinScreen(true);
        // KEIN Invoke(ReturnToMenu) mehr; das übernimmt nun der Credits-Screen
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        CurrentState = GameState.MainMenu;
        showEndScreen = false;
        endMessage = "";
        EnablePlayerControl(false);
        LockCursor(false);
        ShowDeathMenu(false);
        ShowWinScreen(false);
    }

    /// <summary>
    /// Wird vom Todes-Menü-Button &quot;Weiter spielen&quot; aufgerufen.
    /// </summary>
    public void OnDeathMenuContinue()
    {
        // Spieler respawnen
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var behaviour = player.GetComponent<PlayerBehaviour>();
            if (behaviour != null)
            {
                // Position setzen
                if (respawnPoint != null)
                    player.transform.position = respawnPoint.position;

                // Leben auffüllen und Todes-Flag zurücksetzen
                behaviour.currentHealth = behaviour.maxHealth;
                behaviour.isDead = false;

                // Optional: weitere Flags zurücksetzen, falls nötig
                behaviour.isAttacked = false;
            }
        }

        CurrentState = GameState.Playing;
        showEndScreen = false;
        endMessage = "";

        ShowDeathMenu(false);
        EnablePlayerControl(true);
        LockCursor(true);
    }

    /// <summary>
    /// Wird vom Todes-Menü-Button &quot;Zum Hauptmenü&quot; aufgerufen.
    /// </summary>
    public void OnDeathMenuReturnToMainMenu()
    {
        ReturnToMenu();
    }

    private void ShowDeathMenu(bool show)
    {
        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(show);
    }

    private void ShowWinScreen(bool show)
    {
        if (winScreenPanel != null)
            winScreenPanel.SetActive(show);
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

    private void LockCursor(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    /// <summary>
    /// Vom Credits-Script aufrufen, wenn der Scroll durchgelaufen ist.
    /// </summary>
    public void OnCreditsFinished()
    {
        if (CurrentState == GameState.Won)
        {
            ReturnToMenu();
        }
    }
}
