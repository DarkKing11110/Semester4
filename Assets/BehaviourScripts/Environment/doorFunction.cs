using System;
using System.Collections.Generic;
using UnityEngine;

public class doorFunction : MonoBehaviour
{
    public enum SlideDirection { Left, Right, Up, Down, Forward, Back }

    public enum AutoReactMode
    {
        Off,
        PlayerOnly,
        EnemyOnly,
        PlayerOrEnemy
    }

    [Header("Öffnungsrichtung & Distanz")]
    public SlideDirection slideDirection = SlideDirection.Right;
    public float slideDistance = 2f;

    [Header("Geschwindigkeit")]
    public float moveSpeed = 3f;

    [Header("Startzustand")]
    public bool startOpened = false;

    [Header("Eingabe")]
    [Tooltip("Taste zum Öffnen/Schließen (optional).")]
    public string openKey = "E";

    [Header("Berechtigte Entitäten")]
    [Tooltip("Tag des Spielers (standardmäßig 'Player').")]
    public string playerTag = "Player";
    [Tooltip("Tag von KI-Gegnern (z. B. 'Enemy').")]
    public string enemyTag = "Enemy";
    [Tooltip("Explizite GameObjects, die zusätzlich die Tür benutzen dürfen (optional).")]
    public GameObject[] allowedEntities;
    [Tooltip("Zusätzliche Tags, die zusätzlich die Tür benutzen dürfen (optional).")]
    public string[] allowedTags;

    [Header("Reichweite")]
    [Tooltip("Erfordert Anwesenheit einer berechtigten Entität im Triggerbereich (Collider isTrigger=true).")]
    public bool requireEntityInRange = true;
    [Tooltip("Automatik-Modus für Öffnen/Schließen basierend auf Präsenz.")]
    public AutoReactMode autoReactMode = AutoReactMode.PlayerOrEnemy;

    [Header("Trigger (optional extern)")]
    public Collider triggerArea;

    [Header("Doppeltür (optional)")]
    [Tooltip("Zweite Tür, die sich gegensätzlich zur Haupttür bewegen soll (optional).")]
    public Transform secondDoorTransform;

    [Header("Debug Anzeige")]
    public bool showGizmos = true;

    [Header("Debug Steuerung")]
    public bool editorForceOpen = false;
    public bool editorForcePersist = false;

    [Header("Basisposition Wartung")]
    public bool resetClosedPosition = false;

    // Intern Haupttür
    private Vector3 _closedPos;
    private Vector3 _openPos;
    public bool _isOpen;
    private KeyCode _openKeyCode;
    private readonly HashSet<GameObject> _authorizedInRange = new HashSet<GameObject>();
    private bool _closedPosInitialized = false;

    // Intern Zweittür
    private Vector3 _secondClosedPos;
    private Vector3 _secondOpenPos;
    private bool _secondClosedPosInitialized = false;

    void Awake()
    {
        InitClosedPosIfNeeded();
        InitSecondClosedPosIfNeeded();

        RecalculateOpenPos();
        RecalculateSecondOpenPos();

        _isOpen = startOpened;

        // Anfangspositionen anwenden
        transform.localPosition = _isOpen ? _openPos : _closedPos;
        if (secondDoorTransform != null)
            secondDoorTransform.localPosition = _isOpen ? _secondOpenPos : _secondClosedPos;

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
        if (requireEntityInRange)
            RefreshAuthorizedInRange();

        if (requireEntityInRange && autoReactMode != AutoReactMode.Off)
        {
            bool anyAuthorized = HasAuthorizedForAutoReact();
            if (anyAuthorized != _isOpen)
                _isOpen = anyAuthorized;
        }

        HandleInput();
        MoveDoor();
    }

    private bool HasAuthorizedForAutoReact()
    {
        if (_authorizedInRange.Count == 0) return false;

        foreach (var go in _authorizedInRange)
        {
            bool isPlayer = !string.IsNullOrWhiteSpace(playerTag) && go.CompareTag(playerTag);
            bool isEnemy  = !string.IsNullOrWhiteSpace(enemyTag)  && go.CompareTag(enemyTag);

            switch (autoReactMode)
            {
                case AutoReactMode.PlayerOnly:
                    if (isPlayer) return true;
                    break;
                case AutoReactMode.EnemyOnly:
                    if (isEnemy) return true;
                    break;
                case AutoReactMode.PlayerOrEnemy:
                    if (isPlayer || isEnemy) return true;
                    break;
            }
        }
        return false;
    }

    // HandleInput(): manuelles Öffnen im Modus EnemyOnly erlauben, wenn der Spieler real in der Zone ist
    private void HandleInput()
    {
        if (!IsOpenKeyPressed()) return;

        Debug.Log("Open");
        _isOpen = !_isOpen;
        if (requireEntityInRange)
        {
            if (autoReactMode == AutoReactMode.EnemyOnly)
            {
                // explizite Spieler-Zonenprüfung unabhängig von _authorizedInRange
                if (!IsPlayerInZone()) return;
            }
            else
            {
                if (_authorizedInRange.Count == 0) return;
            }
        }

    }

    private void MoveDoor()
    {
        if (_isOpen) Debug.Log("opening"); ;
        Vector3 targetMain = _isOpen ? _openPos : _closedPos;
        if (moveSpeed <= 0f)
            transform.localPosition = targetMain;
        else
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetMain, moveSpeed * Time.deltaTime);

        if (secondDoorTransform != null)
        {
            Vector3 targetSecond = _isOpen ? _secondOpenPos : _secondClosedPos;
            if (moveSpeed <= 0f)
                secondDoorTransform.localPosition = targetSecond;
            else
                secondDoorTransform.localPosition = Vector3.MoveTowards(secondDoorTransform.localPosition, targetSecond, moveSpeed * Time.deltaTime);
        }
    }

    private bool IsOpenKeyPressed()
    {
        if (_openKeyCode == KeyCode.None) return false;
        Debug.Log("pressed");
        return Input.GetKeyDown(_openKeyCode);
    }

    private void ParseKey()
    {
        _openKeyCode = KeyCode.None;
        if (string.IsNullOrWhiteSpace(openKey)) return;

        string key = openKey.Trim();
        if (Enum.TryParse(key, true, out KeyCode parsed)) { _openKeyCode = parsed; return; }

        if (key.Length == 1)
        {
            char c = char.ToUpperInvariant(key[0]);
            if (c >= 'A' && c <= 'Z' && Enum.TryParse(c.ToString(), out parsed)) { _openKeyCode = parsed; return; }
            if (char.IsDigit(c))
            {
                string alphaName = "Alpha" + c;
                if (Enum.TryParse(alphaName, out parsed)) _openKeyCode = parsed;
            }
        }
        else
        {
            Debug.LogWarning($"[doorFunction] Konnte Taste '{openKey}' nicht zuordnen.");
        }
    }

    // RefreshAuthorizedInRange(): Trigger berücksichtigen
    private void RefreshAuthorizedInRange()
    {
        _authorizedInRange.Clear();

        List<Collider> sources = new List<Collider>();
        if (triggerArea != null && triggerArea.isTrigger)
            sources.Add(triggerArea);
        else
        {
            var all = GetComponentsInChildren<Collider>(true);
            foreach (var c in all) if (c != null && c.isTrigger) sources.Add(c);
            if (sources.Count == 0) return;
        }

        foreach (var src in sources)
        {
            if (src == null) continue;
            if (src is BoxCollider box)
            {
                Vector3 center = box.transform.TransformPoint(box.center);
                Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
                var overlaps = Physics.OverlapBox(center, halfExtents, box.transform.rotation, ~0, QueryTriggerInteraction.Collide);
                AddAuthorized(overlaps);
            }
            else if (src is SphereCollider sphere)
            {
                Vector3 center = sphere.transform.TransformPoint(sphere.center);
                float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);
                var overlaps = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide);
                AddAuthorized(overlaps);
            }
            else if (src is CapsuleCollider cap)
            {
                Vector3 center = cap.transform.TransformPoint(cap.center);
                Vector3 axis = cap.direction == 0 ? cap.transform.right : (cap.direction == 1 ? cap.transform.up : cap.transform.forward);
                float radius = cap.radius * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float heightWorld = cap.height * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float halfSegment = Mathf.Max(0f, (heightWorld * 0.5f) - radius);
                Vector3 p0 = center - axis * halfSegment;
                Vector3 p1 = center + axis * halfSegment;
                var overlaps = Physics.OverlapCapsule(p0, p1, radius, ~0, QueryTriggerInteraction.Collide);
                AddAuthorized(overlaps);
            }
            else
            {
                var b = src.bounds;
                var overlaps = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
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

    private GameObject GetEntityRoot(Collider other)
    {
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

    private void RecalculateSecondOpenPos()
    {
        if (secondDoorTransform == null) return;
        float dist = Mathf.Abs(slideDistance);

        // Gegengesetzte Richtung relativ zur zweiten Tür (lokale Achsen der zweiten Tür)
        Vector3 oppositeLocalDir;
        switch (slideDirection)
        {
            case SlideDirection.Left:    oppositeLocalDir = Vector3.right;   break;
            case SlideDirection.Right:   oppositeLocalDir = Vector3.left;    break;
            case SlideDirection.Up:      oppositeLocalDir = Vector3.down;    break;
            case SlideDirection.Down:    oppositeLocalDir = Vector3.up;      break;
            case SlideDirection.Forward: oppositeLocalDir = Vector3.back;    break;
            case SlideDirection.Back:    oppositeLocalDir = Vector3.forward; break;
            default:                     oppositeLocalDir = Vector3.left;    break;
        }

        _secondOpenPos = _secondClosedPos + oppositeLocalDir * dist;
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

    private void InitSecondClosedPosIfNeeded()
    {
        if (secondDoorTransform == null) return;
        if (!_secondClosedPosInitialized)
        {
            _secondClosedPos = secondDoorTransform.localPosition;
            _secondClosedPosInitialized = true;
        }
    }

    private bool IsEntityAuthorized(GameObject go)
    {
        if (!string.IsNullOrWhiteSpace(playerTag) && go.CompareTag(playerTag)) return true;
        if (!string.IsNullOrWhiteSpace(enemyTag)  && go.CompareTag(enemyTag))  return true;
        return IsInAllowedLists(go);
    }

    private bool IsInAllowedLists(GameObject go)
    {
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

    // NEU: Hilfsfunktion – prüft explizit, ob der Spieler in der Trigger-Area ist (ohne _authorizedInRange zu benötigen)
    private bool IsPlayerInZone()
    {
        if (string.IsNullOrWhiteSpace(playerTag)) return false;

        List<Collider> sources = new List<Collider>();
        if (triggerArea != null && triggerArea.isTrigger)
            sources.Add(triggerArea);
        else
        {
            var all = GetComponentsInChildren<Collider>(true);
            foreach (var c in all) if (c != null && c.isTrigger) sources.Add(c);
            if (sources.Count == 0) return false;
        }

        foreach (var src in sources)
        {
            if (src == null) continue;

            if (src is BoxCollider box)
            {
                Vector3 center = box.transform.TransformPoint(box.center);
                Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
                var overlaps = Physics.OverlapBox(center, halfExtents, box.transform.rotation, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < overlaps.Length; i++)
                {
                    var root = GetEntityRoot(overlaps[i]);
                    if (root != null && root.CompareTag(playerTag))
                        return true;
                }
            }
            else if (src is SphereCollider sphere)
            {
                Vector3 center = sphere.transform.TransformPoint(sphere.center);
                float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);
                var overlaps = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < overlaps.Length; i++)
                {
                    var root = GetEntityRoot(overlaps[i]);
                    if (root != null && root.CompareTag(playerTag))
                        return true;
                }
            }
            else if (src is CapsuleCollider cap)
            {
                Vector3 center = cap.transform.TransformPoint(cap.center);
                Vector3 axis = cap.direction == 0 ? cap.transform.right : (cap.direction == 1 ? cap.transform.up : cap.transform.forward);
                float radius = cap.radius * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float heightWorld = cap.height * Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.y, cap.transform.lossyScale.z);
                float halfSegment = Mathf.Max(0f, (heightWorld * 0.5f) - radius);
                Vector3 p0 = center - axis * halfSegment;
                Vector3 p1 = center + axis * halfSegment;
                var overlaps = Physics.OverlapCapsule(p0, p1, radius, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < overlaps.Length; i++)
                {
                    var root = GetEntityRoot(overlaps[i]);
                    if (root != null && root.CompareTag(playerTag))
                        return true;
                }
            }
            else
            {
                var b = src.bounds;
                var overlaps = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
                for (int i = 0; i < overlaps.Length; i++)
                {
                    var root = GetEntityRoot(overlaps[i]);
                    if (root != null && root.CompareTag(playerTag))
                        return true;
                }
            }
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        InitClosedPosIfNeeded();
        InitSecondClosedPosIfNeeded();

        if (resetClosedPosition)
        {
            _closedPos = transform.localPosition;
            _closedPosInitialized = true;
            if (secondDoorTransform != null)
            {
                _secondClosedPos = secondDoorTransform.localPosition;
                _secondClosedPosInitialized = true;
            }
            resetClosedPosition = false;
        }

        RecalculateOpenPos();
        RecalculateSecondOpenPos();

        ApplyEditorForceState();
        ParseKey();
    }

    private void ApplyEditorForceState()
    {
        if (!Application.isPlaying || editorForcePersist)
        {
            _isOpen = editorForceOpen;
            transform.localPosition = _isOpen ? _openPos : _closedPos;
            if (secondDoorTransform != null)
                secondDoorTransform.localPosition = _isOpen ? _secondOpenPos : _secondClosedPos;
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

        // Optionale Gizmos für zweite Tür
        if (secondDoorTransform != null)
        {
            Vector3 secondBaseClosed = Application.isPlaying ? _secondClosedPos : (_secondClosedPosInitialized ? _secondClosedPos : secondDoorTransform.localPosition);
            Vector3 secondOpen = secondBaseClosed + (-dir) * dist; // visuell: gegensätzlich zur Haupttür
            Gizmos.color = Color.cyan;
            Vector3 secondWorldOpen = secondDoorTransform.parent != null ? secondDoorTransform.parent.TransformPoint(secondOpen) : secondOpen;
            Vector3 secondSize = (secondDoorTransform.GetComponent<Renderer>() != null ? secondDoorTransform.GetComponent<Renderer>().bounds.size : Vector3.one * 0.5f);
            Gizmos.DrawWireCube(secondWorldOpen, secondSize);

            Gizmos.color = Color.yellow;
            Vector3 secondWorldClosed = secondDoorTransform.parent != null ? secondDoorTransform.parent.TransformPoint(secondBaseClosed) : secondBaseClosed;
            Gizmos.DrawLine(secondWorldClosed, secondWorldOpen);
        }

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

