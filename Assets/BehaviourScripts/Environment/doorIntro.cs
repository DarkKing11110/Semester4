using System.Collections.Generic;
using UnityEngine;

public class doorIntro : MonoBehaviour
{
    [Header("Intro Tür / Animation")]
    [Tooltip("Animator der Intro-Tür (wird automatisch vom gleichen Objekt geholt, wenn leer).")]
    public Animator introAnimator;
    [Tooltip("Animator-Trigger oder Bool für das Abspielen der Intro-Animation.")]
    public string introAnimTrigger = "PlayIntro";
    [Tooltip("Wenn true, wird statt Trigger ein Bool gesetzt.")]
    public bool useAnimatorBool = false;

    [Header("Minigame Bedingung")]
    [Tooltip("Muss true sein, damit Intro-Animation und Tausch ausgelöst werden dürfen.")]
    public bool minigameSolved = false;
    [Tooltip("Optional: Tag, den der Spieler haben muss, um Trigger auszulösen.")]
    public string playerTag = "Player";

    [Header("Trigger Zonen")]
    [Tooltip("Trigger für das Auslösen der Intro-Animation (externer Collider).")]
    public Collider introTriggerArea;
    [Tooltip("Trigger für den Wechsel zur normalen Tür (externer Collider). Wird erst aktiviert, wenn die Intro-Area betreten wurde.")]
    public Collider swapTriggerArea;

    [Header("Tür-Tausch")]
    [Tooltip("Normale Tür Teil 1 (wird beim Swap aktiviert).")]
    public GameObject normalDoorPartA;
    [Tooltip("Normale Tür Teil 2 (wird beim Swap aktiviert).")]
    public GameObject normalDoorPartB;
    [Tooltip("Intro-Tür-Objekt (wird deaktiviert). Wenn leer, wird dieses GameObject verwendet.")]
    public GameObject introDoorToDeactivate;

    [Header("Filter (optional)")]
    [Tooltip("Explizite GameObjects, die die Trigger auslösen dürfen (optional).")]
    public GameObject[] allowedEntities;
    [Tooltip("Zusätzliche Tags, die die Trigger auslösen dürfen (optional).")]
    public string[] allowedTags;

    private bool introPlayedOnce;
    private bool swapAreaActivated;
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

    // Extern aufrufbar: wenn Minigame gelöst wurde (z. B. vom Puzzle-Controller)
    public void SetMinigameSolved(bool solved) => minigameSolved = solved;

    // Relay-Setup: hängt Weiterleitungs-Komponenten an die externen Trigger
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

        introPlayedOnce = true;
    }

    private void TrySwapDoors()
    {
        if (!minigameSolved) return;

        // Intro-Tür ausblenden
        if (introDoorToDeactivate != null)
            introDoorToDeactivate.SetActive(false);

        // Haupttür-Teile einblenden
        bool anyAssigned = false;
        if (normalDoorPartA != null) { normalDoorPartA.SetActive(true); anyAssigned = true; }
        if (normalDoorPartB != null) { normalDoorPartB.SetActive(true); anyAssigned = true; }

        if (!anyAssigned)
            Debug.LogWarning("[doorIntro] Keine normalen Türteile zugewiesen (normalDoorPartA/B).");
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
