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

    [Header("Trigger (optional extern)")]
    [Tooltip("Optional: Collider eines externen GameObjects, das als Triggerzone dienen soll. Wenn leer, wird ein Trigger am Türobjekt/Kindern erwartet.")]
    public Collider triggerArea;

    [Header("Debug Anzeige")]
    [Tooltip("Zeigt in der Szene die offene Position als Drahtwürfel.")]
    public bool showGizmos = true;

    [Header("Debug Steuerung")]
    [Tooltip("Erzwingt im Editor / Laufzeit den Zustand (offen = true, geschlossen = false).")]
    public bool editorForceOpen = false;
    [Tooltip("Wenn aktiv, wird der erzwungene Zustand jeden Frame (nur Editor) beibehalten).")]
    public bool editorForcePersist = false;

    [Header("Basisposition Wartung")]
    [Tooltip("Setzen um aktuelle LocalPosition als neue geschlossene Basis zu speichern (wird danach wieder deaktiviert).")]
    public bool resetClosedPosition = false;

    // Interne Zustände
    private Vector3 _closedPos;
    private Vector3 _openPos;
    private bool _isOpen;
    private KeyCode _openKeyCode;

    // Tracking berechtigter Entitäten in Reichweite
    private readonly HashSet<GameObject> _authorizedInRange = new HashSet<GameObject>();

    private bool _closedPosInitialized = false;

    void Awake()
    {
        InitClosedPosIfNeeded();
        RecalculateOpenPos();
        _isOpen = startOpened;
        transform.localPosition = _isOpen ? _openPos : _closedPos;
        ParseKey();
#if UNITY_EDITOR
        ApplyEditorForceState();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (editorForcePersist) ApplyEditorForceState();
#endif
        // Bereich prüfen (statt OnTriggerEnter/Exit oder Relay)
        if (requireEntityInRange)
            RefreshAuthorizedInRange();

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

    /// Bereichsprüfung: füllt _authorizedInRange über Physics.Overlap… basierend auf triggerArea oder lokalen Trigger-Collidern
    private void RefreshAuthorizedInRange()
    {
        _authorizedInRange.Clear();

        // Quelle bestimmen: explizite triggerArea oder alle isTrigger-Colliders am Objekt/Kindern
        List<Collider> sources = new List<Collider>();
        if (triggerArea != null && triggerArea.isTrigger)
        {
            sources.Add(triggerArea);
        }
        else
        {
            var all = GetComponentsInChildren<Collider>(true);
            foreach (var c in all)
            {
                if (c != null && c.isTrigger)
                    sources.Add(c);
            }
            if (sources.Count == 0)
            {
                // wenn keine Trigger vorhanden, nichts zu prüfen
                return;
            }
        }

        foreach (var src in sources)
        {
            if (src == null) continue;
            // Overlap je nach Collider-Typ
            if (src is BoxCollider box)
            {
                Vector3 center = box.transform.TransformPoint(box.center);
                Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
                Quaternion rot = box.transform.rotation;
                var overlaps = Physics.OverlapBox(center, halfExtents, rot, ~0, QueryTriggerInteraction.Ignore);
                AddAuthorized(overlaps);
            }
            else if (src is SphereCollider sphere)
            {
                Vector3 center = sphere.transform.TransformPoint(sphere.center);
                float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);
                var overlaps = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Ignore);
                AddAuthorized(overlaps);
            }
            else if (src is CapsuleCollider cap)
            {
                // Endpunkte berechnen
                Vector3 center = cap.transform.TransformPoint(cap.center);
                Vector3 axis;
                switch (cap.direction)
                {
                    case 0: axis = cap.transform.right; break;
                    case 1: axis = cap.transform.up; break;
                    default: axis = cap.transform.forward; break;
                }
                float radius = cap.radius * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float heightWorld = cap.height * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float halfSegment = Mathf.Max(0f, (heightWorld * 0.5f) - radius);
                Vector3 p0 = center - axis * halfSegment;
                Vector3 p1 = center + axis * halfSegment;
                var overlaps = Physics.OverlapCapsule(p0, p1, radius, ~0, QueryTriggerInteraction.Ignore);
                AddAuthorized(overlaps);
            }
            else
            {
                // generischer Fallback über Bounds
                var b = src.bounds;
                var overlaps = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                AddAuthorized(overlaps);
            }
        }
    }

    private void AddAuthorized(Collider[] overlaps)
    {
        if (overlaps == null) return;
        for (int i = 0; i < overlaps.Length; i++)
        {
            var other = overlaps[i];
            if (other == null) continue;
            var root = GetEntityRoot(other);
            if (root == null) continue;
            if (IsEntityAuthorized(root))
                _authorizedInRange.Add(root);
        }
    }

    // Root-Entity bestimmen (Rigidbody-Root bevorzugen)
    private GameObject GetEntityRoot(Collider other)
    {
        if (other == null) return null;
        var rb = other.attachedRigidbody;
        return rb != null ? rb.gameObject : other.gameObject;
    }

    private Vector3 GetLocalSlideVector()
    {
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
        float dist = Mathf.Abs(slideDistance);
        Vector3 dir = GetLocalSlideVector();
        _openPos = _closedPos + dir * dist;
    }

    private void ValidateColliders()
    {
        Collider col = GetComponent<Collider>();
        if (requireEntityInRange && (col == null || !col.isTrigger) && triggerArea == null)
        {
            Debug.LogWarning("[doorFunction] Trigger vorausgesetzt: Füge einen Collider mit isTrigger=true hinzu oder weise 'triggerArea' ein externes Collider-Objekt zu.");
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
        // Keine Relay-Erstellung mehr
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

        if (triggerArea != null)
        {
            Gizmos.color = Color.green;
            var trg = triggerArea;
            if (trg is BoxCollider box)
            {
                var center = trg.transform.TransformPoint(box.center);
                var sizeWorld = Vector3.Scale(box.size, trg.transform.lossyScale);
                Gizmos.DrawWireCube(center, sizeWorld);
            }
            else if (trg is SphereCollider sphere)
            {
                var center = trg.transform.TransformPoint(sphere.center);
                float radiusWorld = sphere.radius * Mathf.Max(trg.transform.lossyScale.x, trg.transform.lossyScale.y, trg.transform.lossyScale.z);
                Gizmos.DrawWireSphere(center, radiusWorld);
            }
            else if (trg is CapsuleCollider cap)
            {
                var center = trg.transform.TransformPoint(cap.center);
                float radiusWorld = cap.radius * Mathf.Max(trg.transform.lossyScale.x, trg.transform.lossyScale.y, trg.transform.lossyScale.z);
                Gizmos.DrawWireSphere(center + Vector3.up * (cap.height * 0.5f - radiusWorld), radiusWorld);
                Gizmos.DrawWireSphere(center - Vector3.up * (cap.height * 0.5f - radiusWorld), radiusWorld);
            }
        }
    }
}

/// <summary>
/// Relay-Komponente: leitet Trigger-Ereignisse von externen/Kind-Collidern an die doorFunction weiter.
/// Wird bei Bedarf automatisch an das Collider-GameObject angefügt.
/// </summary>

