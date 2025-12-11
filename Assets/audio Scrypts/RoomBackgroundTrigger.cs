using UnityEngine;

/// <summary>
/// Legt einen Wert für den Background-Raum-Parameter fest, wenn der Spieler den Bereich betritt/verlässt.
/// Mehrere solcher Trigger können dieselbe Background-Instanz bedienen.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomBackgroundTrigger : MonoBehaviour
{
    [Header("Manager")]
    [Tooltip("Referenz zum BackgroundAudioManager im Level.")]
    public BackgroundAudioManager manager;

    [Header("Trigger Filter")]
    [Tooltip("Tag, das den Spieler identifiziert.")]
    public string playerTag = "Player";

    [Header("Raum-Parameter")]
    [Tooltip("Wert für den Raumzustand beim Betreten.")]
    [Range(0f, 1f)] public float enterValue = 0.5f;
    [Tooltip("Beim Verlassen auf Standard zurücksetzen.")]
    public bool resetOnExit = true;

    private void Reset()
    {
        // Versuche, Manager automatisch zu finden
        manager = FindObjectOfType<BackgroundAudioManager>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || manager == null) return;
        manager.SetRoomParameter(enterValue);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || manager == null) return;
        if (resetOnExit) manager.ResetToDefault();
    }
}