using System;
using System.Collections.Generic;
using UnityEngine;

public class doorFunction : MonoBehaviour
{
    // Alle möglichen lokalen Richtungen
    public enum SlideDirection
    {
        Left,   // -X
        Right,  // +X
        Up,     // +Y
        Down,   // -Y
        Forward,// +Z
        Back    // -Z
    }

    [Header("Öffnungsrichtung & Distanz")]
    [Tooltip("Lokale Richtung, in die sich die Tür beim Öffnen verschiebt.")]
    public SlideDirection slideDirection = SlideDirection.Right;
    [Tooltip("Wie weit die Tür sich in die gewählte Richtung verschiebt (Meter, Vorzeichen egal).")]
    public float slideDistance = 2f;

    [Header("Geschwindigkeit")]
    [Tooltip("Geschwindigkeit der Bewegung in Metern pro Sekunde.")]
    public float moveSpeed = 3f;

    [Header("Startzustand")]
    [Tooltip("Falls aktiviert, startet die Tür direkt im geöffneten Zustand.")]
    public bool startOpened = false;

    [Header("Eingabe")]
    [Tooltip("Taste zum Öffnen/Schließen (Groß-/Kleinschreibung egal). Beispiele: E, F, Space, Return, Alpha1.")]
    public string openKey = "E";

    [Header("Berechtigte Entitäten")]
    [Tooltip("Explizite GameObjects, die die Tür benutzen dürfen (optional).")]
    public GameObject[] allowedEntities;
    [Tooltip("Tags von Objekten, die die Tür benutzen dürfen (optional).")]
    public string[] allowedTags;

    [Header("Reichweite")]
    [Tooltip("Erfordert Anwesenheit einer berechtigten Entität im Triggerbereich (Collider isTrigger=true).")]
    public bool requireEntityInRange = true;

    [Header("Debug Anzeige")]
    [Tooltip("Zeigt in der Szene die offene Position als Drahtwürfel.")]
    public bool showGizmos = true;

    [Header("Debug Steuerung")]
    [Tooltip("Erzwingt im Editor / Laufzeit den Zustand (offen = true, geschlossen = false).")]
    public bool editorForceOpen = false;
    [Tooltip("Wenn aktiv, wird der erzwungene Zustand jeden Frame (nur Editor) beibehalten.")]
    public bool editorForcePersist = false;

    [Header("Basisposition Wartung")]
    [Tooltip("Setzen um aktuelle LocalPosition als neue geschlossene Basis zu speichern (wird danach wieder deaktiviert).")]
    public bool resetClosedPosition = false;

    // Interne Zustände
    private Vector3 _closedPos;
    private Vector3 _openPos;
    private bool _isOpen;
    private KeyCode _openKeyCode;
    private readonly HashSet<Collider> _authorizedInRange = new HashSet<Collider>();
    private bool _closedPosInitialized = false;

    void Awake()
    {
        InitClosedPosIfNeeded();
        RecalculateOpenPos();
        _isOpen = startOpened;
        transform.localPosition = _isOpen ? _openPos : _closedPos;
        ParseKey();
        ValidateColliders();
#if UNITY_EDITOR
        ApplyEditorForceState();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (editorForcePersist) ApplyEditorForceState();
#endif
        HandleInput();
        MoveDoor();
    }

    private void HandleInput()
    {
        if (!IsOpenKeyPressed()) return;
        if (requireEntityInRange && _authorizedInRange.Count == 0) return;
        _isOpen = !_isOpen;
    }

    private void MoveDoor()
    {
        Vector3 target = _isOpen ? _openPos : _closedPos;
        if (moveSpeed <= 0f)
        {
            transform.localPosition = target;
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, moveSpeed * Time.deltaTime);
        }
    }

    private bool IsOpenKeyPressed()
    {
        if (_openKeyCode == KeyCode.None) return false;
        return Input.GetKeyDown(_openKeyCode);
    }

    private void ParseKey()
    {
        _openKeyCode = KeyCode.None;
        if (string.IsNullOrWhiteSpace(openKey)) return;

        string key = openKey.Trim();

        if (Enum.TryParse(key, true, out KeyCode parsed))
        {
            _openKeyCode = parsed;
            return;
        }

        if (key.Length == 1)
        {
            char c = char.ToUpperInvariant(key[0]);
            if (c >= 'A' && c <= 'Z')
            {
                if (Enum.TryParse(c.ToString(), out parsed))
                    _openKeyCode = parsed;
                return;
            }
            if (char.IsDigit(c))
            {
                string alphaName = "Alpha" + c;
                if (Enum.TryParse(alphaName, out parsed))
                    _openKeyCode = parsed;
                return;
            }
        }

        Debug.LogWarning($"[doorFunction] Konnte Taste '{openKey}' nicht zuordnen. Verwende gültigen KeyCode oder Einzelzeichen.");
    }

    private void ValidateColliders()
    {
        Collider col = GetComponent<Collider>();
        if (requireEntityInRange && (col == null || !col.isTrigger))
        {
            Debug.LogWarning("[doorFunction] Trigger vorausgesetzt: Füge einen Collider mit isTrigger=true hinzu.");
        }
    }

    private void InitClosedPosIfNeeded()
    {
        if (!_closedPosInitialized)
        {
            _closedPos = transform.localPosition;
            _closedPosInitialized = true;
        }
    }

    private Vector3 GetLocalSlideVector()
    {
        // Reine lokale Richtungsvektoren (unabhängig von Rotation)
        switch (slideDirection)
        {
            case SlideDirection.Left:    return Vector3.left;
            case SlideDirection.Right:   return Vector3.right;
            case SlideDirection.Up:      return Vector3.up;
            case SlideDirection.Down:    return Vector3.down;
            case SlideDirection.Forward: return Vector3.forward;
            case SlideDirection.Back:    return Vector3.back;
            default: return Vector3.right;
        }
    }

    private void RecalculateOpenPos()
    {
        float dist = Mathf.Abs(slideDistance); // Vorzeichen ignorieren, Richtung über Enum
        Vector3 dir = GetLocalSlideVector();
        _openPos = _closedPos + dir * dist;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!requireEntityInRange) return;
        if (IsEntityAuthorized(other.gameObject))
            _authorizedInRange.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!requireEntityInRange) return;
        _authorizedInRange.Remove(other);
    }

    private bool IsEntityAuthorized(GameObject go)
    {
        bool hasSpecifics = (allowedEntities != null && allowedEntities.Length > 0) ||
                            (allowedTags != null && allowedTags.Length > 0);
        if (!hasSpecifics) return true;

        if (allowedEntities != null)
        {
            for (int i = 0; i < allowedEntities.Length; i++)
            {
                if (allowedEntities[i] == null) continue;
                if (go == allowedEntities[i]) return true;
            }
        }

        if (allowedTags != null)
        {
            for (int i = 0; i < allowedTags.Length; i++)
            {
                string tag = allowedTags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (go.CompareTag(tag)) return true;
            }
        }
        return false;
    }

    public void Open() => _isOpen = true;
    public void Close() => _isOpen = false;
    public void Toggle() => _isOpen = !_isOpen;
    public bool IsOpen => _isOpen;

#if UNITY_EDITOR
    private void OnValidate()
    {
        InitClosedPosIfNeeded();

        if (resetClosedPosition)
        {
            _closedPos = transform.localPosition;
            _closedPosInitialized = true;
            resetClosedPosition = false;
        }

        RecalculateOpenPos();
        ApplyEditorForceState();
        ParseKey();
    }

    private void ApplyEditorForceState()
    {
        if (!Application.isPlaying || editorForcePersist)
        {
            _isOpen = editorForceOpen;
            transform.localPosition = _isOpen ? _openPos : _closedPos;
        }
    }
#endif

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 baseClosed = Application.isPlaying ? _closedPos : (_closedPosInitialized ? _closedPos : transform.localPosition);
        float dist = Mathf.Abs(slideDistance);
        Vector3 dir = GetLocalSlideVector();
        Vector3 openPos = baseClosed + dir * dist;

        Gizmos.color = Color.cyan;
        Vector3 worldOpen = transform.parent != null ? transform.parent.TransformPoint(openPos) : openPos;
        Vector3 size = (GetComponent<Renderer>() != null ? GetComponent<Renderer>().bounds.size : Vector3.one * 0.5f);
        Gizmos.DrawWireCube(worldOpen, size);

        Gizmos.color = Color.yellow;
        Vector3 worldClosed = transform.parent != null ? transform.parent.TransformPoint(baseClosed) : baseClosed;
        Gizmos.DrawLine(worldClosed, worldOpen);
    }
}
