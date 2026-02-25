using UnityEngine;
using UnityEngine.Events;

public class MissionPanelTrigger : MonoBehaviour
{
    [Header("Verhalten")]
    [Tooltip("Ignoriert das erste Aktivieren (z. B. beim Szenenstart, wenn das Panel bereits aktiv ist).")]
    public bool ignoreFirstEnable = true;

    [Tooltip("Führe die Aktion nur einmalig aus.")]
    public bool runOnlyOnce = false;

    [Header("Quest-Bedingung")]
    [Tooltip("StoryManager-Referenz (optional, versucht sonst automatisch eine zu finden).")]
    public StoryManager storyManager;
    [Tooltip("Erwartete Quest-ID oder Name (muss zur aktuellen Mission passen).")]
    public string expectedQuestIdOrName;
    [Tooltip("Quest Bedingung")]
    public string Condition;

    [Header("Events")]
    [Tooltip("Wird aufgerufen, wenn das Panel aktiv wird und die Quest-Bedingung erfüllt ist.")]
    public UnityEvent onPanelActivatedMatchingQuest;

    [Tooltip("Wird aufgerufen, wenn das Panel deaktiviert wird.")]
    public UnityEvent onPanelDeactivated;

    private bool _initialized;   // wurde bereits einmal aktiviert
    private bool _ranOnce;       // wurde die Aktion bereits einmal ausgeführt

    private void Awake()
    {
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();
    }

    private void OnEnable()
    {
        // Erstes Aktivieren ggf. ignorieren
        if (ignoreFirstEnable && !_initialized)
        {
            _initialized = true;
            return;
        }

        if (runOnlyOnce && _ranOnce) return;

        // Quest-Bedingung prüfen
        if (IsCorrectQuestActive())
        {
            GameEvents.OnMinigameCompleted?.Invoke(Condition); // Optional: Informiere andere Systeme über die Quest-Aktivierung
            _ranOnce = true;
            onPanelActivatedMatchingQuest?.Invoke();
        }
    }

    private void OnDisable()
    {
        onPanelDeactivated?.Invoke();
    }

    // Prüft, ob die erwartete Quest aktiv ist
    private bool IsCorrectQuestActive()
    {
        if (storyManager == null) return false;
        var currentData = storyManager.CurrentStepData;
        if (currentData == null) return false;

        // Falls dein StoryStepData eine eigene ID/Eigenschaft hat, hier entsprechend vergleichen.
        // Fallback: vergleiche gegen den ScriptableObject-Namen.
        var currentIdOrName = storyManager.GetCurrentId();
        if (string.IsNullOrWhiteSpace(expectedQuestIdOrName)) return false;

        return string.Equals(currentIdOrName, expectedQuestIdOrName, System.StringComparison.OrdinalIgnoreCase);
    }
}
