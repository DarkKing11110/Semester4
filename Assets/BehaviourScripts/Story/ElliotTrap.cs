using System.Collections.Generic;
using UnityEngine;

public class ElliotTrap : MonoBehaviour
{
    [Header("Quest-Bedingung")]
    [Tooltip("StoryManager-Referenz (optional, versucht automatisch zu finden).")]
    public StoryManager storyManager;
    [Tooltip("Erwartete Quest-ID oder Name, damit die Falle auslöst.")]
    public string requiredQuestIdOrName = "ElliotTrapQuest";

    [Header("Entitäten")]
    [Tooltip("Tag des Spielers (z. B. 'Player').")]
    public string playerTag = "Player";
    [Tooltip("Tag des Gegners (z. B. 'Enemy').")]
    public string enemyTag = "Enemy";

    [Header("Zu deaktivierende Scripts")]
    [Tooltip("Erstes Objekt, bei dem ein Script deaktiviert werden soll.")]
    public GameObject targetObject1;
    [Tooltip("Typ-Name des Scripts auf targetObject1 (z. B. 'EnemyMovement').")]
    public string scriptName1 = "EnemyMovement";

    [Tooltip("Zweites Objekt, bei dem ein Script deaktiviert werden soll.")]
    public GameObject targetObject2;
    [Tooltip("Typ-Name des Scripts auf targetObject2 (z. B. 'EnemyMovement').")]
    public string scriptName2 = "EnemyMovement";

    [Header("Trigger Verhalten")]
    [Tooltip("Nur einmalig auslösen (true) oder bei jedem Spieler-Austritt (false).")]
    public bool triggerOnce = true;

    private readonly HashSet<GameObject> _entitiesInZone = new HashSet<GameObject>();
    private bool _trapTriggered;

    private void Awake()
    {
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();

        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[ElliotTrap] Collider sollte isTrigger=true haben.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var root = GetEntityRoot(other);
        if (root == null) return;

        // Nur Spieler oder Gegner tracken
        if (IsPlayer(root) || IsEnemy(root))
            _entitiesInZone.Add(root);
    }

    private void OnTriggerExit(Collider other)
    {
        var root = GetEntityRoot(other);
        if (root == null) return;

        bool wasInZone = _entitiesInZone.Remove(root);

        // Falle auslösen, wenn Spieler die Zone verlässt, Gegner aber noch drin ist
        if (wasInZone && IsPlayer(root))
        {
            TryTriggerTrap();
        }
    }

    private void TryTriggerTrap()
    {
        if (triggerOnce && _trapTriggered) return;
        if (!IsCorrectQuestActive()) return;

        // Prüfen, ob nur noch der Gegner in der Zone ist
        bool enemyPresent = false;
        bool playerPresent = false;

        foreach (var entity in _entitiesInZone)
        {
            if (IsPlayer(entity)) playerPresent = true;
            if (IsEnemy(entity)) enemyPresent = true;
        }

        // Falle nur auslösen, wenn Spieler weg ist und Gegner noch da
        if (!playerPresent && enemyPresent)
        {
            ActivateTrap();
            if (triggerOnce) _trapTriggered = true;
        }
    }

    private void ActivateTrap()
    {
        Debug.Log("[ElliotTrap] Falle ausgelöst: Spieler hat Zone verlassen, Gegner bleibt.");

        // Scripts deaktivieren
        DisableScript(targetObject1, scriptName1);
        DisableScript(targetObject2, scriptName2);
        GameEvents.OnMinigameCompleted?.Invoke("elliotTrap");
    }

    private void DisableScript(GameObject target, string scriptTypeName)
    {
        if (target == null || string.IsNullOrWhiteSpace(scriptTypeName)) return;

        var script = target.GetComponent(scriptTypeName) as MonoBehaviour;
        if (script != null)
        {
            script.enabled = false;
            Debug.Log($"[ElliotTrap] Script '{scriptTypeName}' auf '{target.name}' deaktiviert.");
        }
        else
        {
            Debug.LogWarning($"[ElliotTrap] Script '{scriptTypeName}' auf '{target.name}' nicht gefunden.");
        }
    }

    private GameObject GetEntityRoot(Collider other)
    {
        var rb = other.attachedRigidbody;
        return rb != null ? rb.gameObject : other.gameObject;
    }

    private bool IsPlayer(GameObject go)
    {
        return !string.IsNullOrWhiteSpace(playerTag) && go.CompareTag(playerTag);
    }

    private bool IsEnemy(GameObject go)
    {
        return !string.IsNullOrWhiteSpace(enemyTag) && go.CompareTag(enemyTag);
    }

    private bool IsCorrectQuestActive()
    {
        if (storyManager == null) return false;
        var currentIdOrName = storyManager.GetCurrentMissionIdOrName();
        if (string.IsNullOrWhiteSpace(requiredQuestIdOrName)) return false;

        return string.Equals(currentIdOrName, requiredQuestIdOrName, System.StringComparison.OrdinalIgnoreCase);
    }
}
