using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [Tooltip("Tag des Spielers, der den Missionstrigger auslösen darf.")]
    public string playerTag = "Player";

    [Tooltip("Einmalig auslösen (true) oder bei jedem Eintritt (false).")]
    public bool triggerOnce = true;

    [Tooltip("MissionTrigger String")]
    public string missionTriggerString = "Enter String";

    [Tooltip("MissionTrigger Integer")]
    public int MissionIndex = 0;

    private bool triggered;

    [SerializeField] private StoryManager storyManager;

    private void Reset()
    {
        // Hilft im Editor: kennzeichnet Collider als Trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        // Fallback, falls nicht gesetzt: erste Instanz suchen
        if (storyManager == null) storyManager = FindObjectOfType<StoryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;

        var root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!string.IsNullOrWhiteSpace(playerTag) && !root.CompareTag(playerTag) && storyManager.CurrentStepIndex == MissionIndex) return;

        // Missionsfortschritt auslösen
        AdvanceMission();

        if (triggerOnce) triggered = true;
    }

    // Hier hängst du deine Missionslogik ein (Event, Manager, etc.)
    private void AdvanceMission()
    {
        // Beispiel: globales Event feuern
        // GameEvents.OnMissionStepReached?.Invoke("IntroArea");

        // Oder direkt einen Manager ansprechen
        // MissionManager.Instance?.AdvanceStep("IntroArea");
        GameEvents.OnMinigameCompleted?.Invoke(missionTriggerString);
        Debug.Log("[MissionTrigger] Missionsfortschritt ausgelöst.");
    }
}
