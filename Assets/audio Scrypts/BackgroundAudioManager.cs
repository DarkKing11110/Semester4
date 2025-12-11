using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Spielt ein dauerhaftes Background-Event und steuert die Raum-Atmosphäre über einen FMOD-Parameter.
/// - Hintergrund-Event wird einmal gestartet und bleibt aktiv.
/// - Räume setzen beim Betreten/Verlassen den Parameter auf konfigurierte Werte.
/// </summary>
public class BackgroundAudioManager : MonoBehaviour
{
    [Header("FMOD Background Event")]
    [Tooltip("FMOD StudioEventEmitter, der das Hintergrund-Event abspielt (one-shot start, looping in FMOD).")]
    public StudioEventEmitter backgroundEmitter;

    [Header("Parameter-Steuerung")]
    [Tooltip("Name des FMOD-Parameters, der die Raum-Atmosphäre steuert (z.B. \"RoomType\").")]
    public string roomParameterName = "RoomType";
    [Tooltip("Standardwert, wenn kein Raum aktiv ist (z.B. Korridor/Neutral).")]
    [Range(0f, 1f)] public float defaultRoomValue = 0f;

    [Header("Debug")]
    public bool debugLogs = false;

    private EventInstance _instance;

    void Awake()
    {
        if (backgroundEmitter == null)
        {
            Debug.LogError("[BackgroundAudioManager] Kein StudioEventEmitter zugewiesen.");
            return;
        }

        // Emitter starten (falls noch nicht aktiv) 
        backgroundEmitter.Play();

        _instance = backgroundEmitter.EventInstance;
        if (!_instance.isValid())
        {
            // Versuch erneut nach Play()
            _instance = backgroundEmitter.EventInstance;
        }

        if (_instance.isValid())
        {
            // Parameter initial setzen
            _instance.setParameterByName(roomParameterName, defaultRoomValue);
            if (debugLogs) Debug.Log($"[BackgroundAudioManager] Init parameter {roomParameterName}={defaultRoomValue}");
        }
        else
        {
            Debug.LogWarning("[BackgroundAudioManager] EventInstance nicht valide. Fällt auf globalen Parameter zurück.");
            RuntimeManager.StudioSystem.setParameterByName(roomParameterName, defaultRoomValue);
        }
    }

    /// <summary>
    /// Wird von Raum-Triggern gerufen, um den Parameter zu setzen.
    /// </summary>
    public void SetRoomParameter(float value)
    {
        if (_instance.isValid())
        {
            _instance.setParameterByName(roomParameterName, value);
            if (debugLogs) Debug.Log($"[BackgroundAudioManager] Set instance param {roomParameterName}={value}");
        }
        else
        {
            RuntimeManager.StudioSystem.setParameterByName(roomParameterName, value);
            if (debugLogs) Debug.Log($"[BackgroundAudioManager] Set global param {roomParameterName}={value}");
        }
    }

    /// <summary>
    /// Reset auf Standard, wenn kein Raum aktiv ist.
    /// </summary>
    public void ResetToDefault()
    {
        SetRoomParameter(defaultRoomValue);
    }
}