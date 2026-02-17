using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class doorIntro : MonoBehaviour
{
    [Header("Intro T�r / Animation")]
    [Tooltip("Animator der Intro-T�r (wird automatisch vom gleichen Objekt geholt, wenn leer).")]
    public Animator introAnimator;
    [Tooltip("Animator-Trigger oder Bool f�r das Abspielen der Intro-Animation.")]
    public string introAnimTrigger = "PlayIntro";
    [Tooltip("Wenn true, wird statt Trigger ein Bool gesetzt.")]
    public bool useAnimatorBool = false;

    [Header("Minigame Bedingung")]
    [Tooltip("Muss true sein, damit Intro-Animation und Tausch ausgel�st werden d�rfen.")]
    public bool minigameSolved = false;
    [Tooltip("Optional: Tag, den der Spieler haben muss, um Trigger auszul�sen.")]
    public string playerTag = "Player";

    [Header("Trigger Zonen")]
    [Tooltip("Trigger f�r das Ausl�sen der Intro-Animation (externer Collider).")]
    public Collider introTriggerArea;
    [Tooltip("Trigger f�r den Wechsel zur normalen T�r (externer Collider). Wird erst aktiviert, wenn die Intro-Area betreten wurde.")]
    public Collider swapTriggerArea;

    [Header("T�r-Tausch")]
    [Tooltip("Normale T�r Teil 1 (wird beim Swap aktiviert).")]
    public GameObject normalDoorPartA;
    [Tooltip("Normale T�r Teil 2 (wird beim Swap aktiviert).")]
    public GameObject normalDoorPartB;
    [Tooltip("Intro-T�r-Objekt (wird deaktiviert). Wenn leer, wird dieses GameObject verwendet.")]
    public GameObject introDoorToDeactivate;

    [Header("Filter (optional)")]
    [Tooltip("Explizite GameObjects, die die Trigger ausl�sen d�rfen (optional).")]
    public GameObject[] allowedEntities;
    [Tooltip("Zus�tzliche Tags, die die Trigger ausl�sen d�rfen (optional).")]
    public string[] allowedTags;

    [Header("FMOD Intro Event")]
    [Tooltip("FMOD event that plays once at animation start.")]
    public EventReference introStartEvent;

    private bool introPlayedOnce;
    private bool swapAreaActivated;
    private bool introEventPlayed;
    private readonly HashSet<GameObject> _authorizedInRange = new HashSet<GameObject>();

    void Awake()
    {
        // Animator automatisch vom gleichen Objekt holen, falls nicht gesetzt
        if (introAnimator == null)
            introAnimator = GetComponent<Animator>();

        if (introDoorToDeactivate == null)
            introDoorToDeactivate = gameObject;

        // Swap-Area initial deaktivieren (sofern vorhanden)
        if (swapTriggerArea != null)
        {
            var swapGO = swapTriggerArea.gameObject;
            if (swapGO.activeSelf) swapGO.SetActive(false);
            swapAreaActivated = false;
        }

        SetupTriggerRelays();
    }

    // Extern aufrufbar: wenn Minigame gel�st wurde (z. B. vom Puzzle-Controller)
    public void SetMinigameSolved(bool solved) => minigameSolved = solved;

    // Relay-Setup: h�ngt Weiterleitungs-Komponenten an die externen Trigger
    private void SetupTriggerRelays()
    {
        // Intro-Trigger
        if (introTriggerArea != null)
        {
            if (!introTriggerArea.isTrigger)
                Debug.LogWarning("[doorIntro] introTriggerArea hat isTrigger=false. Setze isTrigger=true.");
            var relay = introTriggerArea.gameObject.GetComponent<DoorIntroTriggerRelay>();
            if (relay == null) relay = introTriggerArea.gameObject.AddComponent<DoorIntroTriggerRelay>();
            relay.owner = this;
            relay.kind = DoorIntroTriggerRelay.TriggerKind.Intro;
        }
        else
        {
            Debug.LogWarning("[doorIntro] introTriggerArea ist nicht zugewiesen.");
        }

        // Swap-Trigger (Relay vorbereiten, Objekt bleibt aber deaktiviert bis Aktivierung)
        if (swapTriggerArea != null)
        {
            if (!swapTriggerArea.isTrigger)
                Debug.LogWarning("[doorIntro] swapTriggerArea hat isTrigger=false. Setze isTrigger=true.");
            var relay = swapTriggerArea.gameObject.GetComponent<DoorIntroTriggerRelay>();
            if (relay == null) relay = swapTriggerArea.gameObject.AddComponent<DoorIntroTriggerRelay>();
            relay.owner = this;
            relay.kind = DoorIntroTriggerRelay.TriggerKind.Swap;
        }
        else
        {
            Debug.LogWarning("[doorIntro] swapTriggerArea ist nicht zugewiesen.");
        }
    }

    // Wird vom Relay aufgerufen
    internal void OnRelayEnter(Collider other, DoorIntroTriggerRelay.TriggerKind kind)
    {
        var root = GetEntityRoot(other);
        if (root == null) return;
        if (!IsEntityAuthorized(root)) return;

        if (kind == DoorIntroTriggerRelay.TriggerKind.Intro)
        {
            // Swap-Area freischalten, sobald Intro-Area betreten wurde (einmalig)
            ActivateSwapAreaIfNeeded();
            TryPlayIntro();
        }
        else if (kind == DoorIntroTriggerRelay.TriggerKind.Swap)
        {
            TrySwapDoors();
        }
    }

    private void ActivateSwapAreaIfNeeded()
    {
        if (swapAreaActivated) return;
        if (swapTriggerArea == null) return;

        var swapGO = swapTriggerArea.gameObject;
        swapGO.SetActive(true);
        swapAreaActivated = true;
    }

    private void TryPlayIntro()
    {
        if (!minigameSolved) return;
        if (introPlayedOnce) return;

        if (introAnimator == null)
        {
            Debug.LogWarning("[doorIntro] Kein Animator auf dem Objekt gefunden.");
            introPlayedOnce = true;
            return;
        }

        if (useAnimatorBool)
            introAnimator.SetBool(introAnimTrigger, true);
        else
            introAnimator.SetTrigger(introAnimTrigger);

        if (!introEventPlayed && !introStartEvent.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(introStartEvent, gameObject);
            introEventPlayed = true;
        }

        introPlayedOnce = true;
    }

    private void TrySwapDoors()
    {
        if (!minigameSolved) return;

        // Intro-T�r ausblenden
        if (introDoorToDeactivate != null)
            introDoorToDeactivate.SetActive(false);

        // Hauptt�r-Teile einblenden
        bool anyAssigned = false;
        if (normalDoorPartA != null) { normalDoorPartA.SetActive(true); anyAssigned = true; }
        if (normalDoorPartB != null) { normalDoorPartB.SetActive(true); anyAssigned = true; }

        if (!anyAssigned)
            Debug.LogWarning("[doorIntro] Keine normalen T�rteile zugewiesen (normalDoorPartA/B).");
    }

    private GameObject GetEntityRoot(Collider other)
    {
        var rb = other.attachedRigidbody;
        return rb != null ? rb.gameObject : other.gameObject;
    }

    private bool IsEntityAuthorized(GameObject go)
    {
        if (!string.IsNullOrWhiteSpace(playerTag) && !go.CompareTag(playerTag))
            return false;

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
                var tag = allowedTags[i];
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (go.CompareTag(tag)) return true;
            }
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Animator auch im Editor automatisch referenzieren
        if (introAnimator == null)
            introAnimator = GetComponent<Animator>();
    }
#endif
}

public sealed class DoorIntroTriggerRelay : MonoBehaviour
{
    public enum TriggerKind { Intro, Swap }

    public doorIntro owner;
    public TriggerKind kind = TriggerKind.Intro;

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null) owner.OnRelayEnter(other, kind);
    }
}
