using UnityEngine;
using FMODUnity;
using FMOD;

[RequireComponent(typeof(Collider))]
public class AmbienceParameterTrigger : MonoBehaviour
{
    [Header("FMOD Parameter")]
    [Tooltip("Name des FMOD-Parameters, der die Umgebungsatmo steuert.")]
    public string parameterName = "AmbienceState";
    [Tooltip("Wert, der gesetzt wird, solange der Spieler im Bereich bleibt.")]
    public float insideValue = 1f;
    [Tooltip("Standardwert, auf den nach Verlassen des Bereichs zurückgeschaltet wird.")]
    public float defaultValue = 0f;

    [Header("Trigger")]
    [Tooltip("Vergleichs-Tag für den Spieler.")]
    public string playerTag = "Player";
    [Tooltip("Falls aktiv, wird der Collider automatisch als Trigger gesetzt.")]
    public bool forceTrigger = true;

    private bool _isQuitting;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        if (forceTrigger)
            EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        ApplyParameter(insideValue);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        ApplyParameter(defaultValue);
    }

    private void OnDisable()
    {
        ApplyParameter(defaultValue);
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void ApplyParameter(float value)
    {
        if (_isQuitting) return;
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            Debug.LogWarning("[AmbienceParameterTrigger] Parametername ist leer. Bitte im Inspector setzen.");
            return;
        }
        var studioSystem = RuntimeManager.StudioSystem;
        if (!studioSystem.isValid())
        {
            Debug.LogWarning("[AmbienceParameterTrigger] FMOD StudioSystem ist nicht initialisiert.");
            return;
        }

        RESULT result = studioSystem.setParameterByName(parameterName, value);
        if (result != RESULT.OK)
        {
            Debug.LogWarning($"[AmbienceParameterTrigger] Konnte Parameter '{parameterName}' nicht setzen: {result}");
        }
    }

    private void EnsureTriggerCollider()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }
}
