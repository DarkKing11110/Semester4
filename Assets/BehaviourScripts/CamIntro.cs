using System.Collections;
using UnityEngine;

public class CamIntro : MonoBehaviour
{
    [Header("Kameras")]
    public Camera introCamera;
    public Camera mainCamera;

    [Header("Ziele/Referenzen")]
    [Tooltip("Root des Spielers (wird genutzt, um Bounds zu finden).")]
    public Transform playerRoot;
    [Tooltip("Optional: explizites Ziel (z.B. Brust/Kopf). Wenn leer, werden Bounds genutzt.")]
    public Transform lookAtTarget;
    [Tooltip("Optional: Startpose der Intro-Kamera. Wenn leer, wird automatisch vor dem Spieler platziert.")]
    public Transform introStartPose;

    [Header("Framing")]
    [Tooltip("Seitlicher Versatz relativ zur Spieler-Vorwärtsrichtung (Meter).")]
    public float lateralOffset = 0f;
    [Tooltip("Vertikaler Offset zusätzlich zu berechneter Höhe (Meter).")]
    public float verticalOffset = 0.2f;
    [Tooltip("Faktor für die Distanz (größer = weiter weg).")]
    public float distanceFactor = 1.1f;
    [Tooltip("Minimale Distanz zur Sicherheit (Meter).")]
    public float minDistance = 2.0f;

    [Header("Ablaufzeiten")]
    public float holdFrontDuration = 1.2f;
    public float shakeDuration = 0.6f;
    public float blendDuration = 1.25f;

    [Header("Schüttel-Effekt")]
    public float shakeAmplitude = 0.08f;
    public float shakeFrequency = 18f;
    public AnimationCurve shakeFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Überblendungskurven")]
    public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sonstiges")]
    public bool playOnStart = true;

    [Header("Spieler-Sperre")]
    [Tooltip("Wenn aktiv, wird die Spielerbewegung während der Intro-Kamera gesperrt.")]
    public bool lockPlayerMovementDuringIntro = true;
    [Tooltip("Optional: direkte Referenz auf PlayerBehaviour. Wenn leer, wird automatisch gesucht.")]
    public PlayerBehaviour playerBehaviour;

    // intern
    private Transform introCamTransform;
    private Vector3 blendStartPos;
    private Quaternion blendStartRot;
    private bool playing;

    void Start()
    {
        if (playOnStart)
            PlayIntro();
    }

    public void PlayIntro()
    {
        if (introCamera == null || mainCamera == null)
        {
            Debug.LogError("[CamIntro] Bitte 'introCamera' und 'mainCamera' zuweisen.");
            return;
        }
        if (playing) return;

        // Spielerbewegung sperren
        if (lockPlayerMovementDuringIntro)
        {
            if (playerBehaviour == null)
                playerBehaviour = FindObjectOfType<PlayerBehaviour>();
            if (playerBehaviour != null)
                playerBehaviour.enabled = false;
        }

        introCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        introCamTransform = introCamera.transform;

        // Startpose setzen: falls nicht vorgegeben, automatisch vor den Spieler platzieren
        if (introStartPose != null)
        {
            introCamTransform.position = introStartPose.position;
            introCamTransform.rotation = introStartPose.rotation;
        }
        else
        {
            AutoPlaceIntroCamera();
        }

        // Blickziel ausrichten
        Transform target = lookAtTarget != null ? lookAtTarget : playerRoot;
        if (target != null)
        {
            Vector3 dir = (GetPlayerCenterWorld() - introCamTransform.position);
            if (dir.sqrMagnitude > 0.0001f)
                introCamTransform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        StartCoroutine(IntroSequence());
    }

    // Automatische Platzierung der Kamera, sodass der ganze Spieler ins Bild passt
    private void AutoPlaceIntroCamera()
    {
        if (playerRoot == null)
        {
            Debug.LogWarning("[CamIntro] 'playerRoot' ist nicht gesetzt. Verwende aktuelle Intro-Kamera-Position.");
            return;
        }

        Bounds b = GetPlayerBounds();
        Vector3 center = b.center;
        float height = Mathf.Max(1.0f, b.size.y);
        float width = Mathf.Max(0.5f, b.size.x);

        // Kamera vor dem Spieler (entlang Spieler-Vorwärts entgegen schauen)
        Vector3 playerForward = playerRoot.forward;
        Vector3 playerRight = playerRoot.right;

        // Distanz basierend auf FOV und Größe abschätzen (einfacher Näherungswert)
        float fovRad = introCamera != null ? introCamera.fieldOfView * Mathf.Deg2Rad : 60f * Mathf.Deg2Rad;
        float aspect = introCamera != null ? introCamera.aspect : 16f / 9f;
        float halfFov = fovRad * 0.5f;

        // Benötigte Distanz, damit Höhe und Breite in den Bildausschnitt passen
        float distForHeight = (height * 0.5f) / Mathf.Tan(halfFov);
        float verticalFovForWidth = 2f * Mathf.Atan(Mathf.Tan(halfFov) / aspect);
        float distForWidth = (width * 0.5f) / Mathf.Tan(verticalFovForWidth * 0.5f);

        float baseDist = Mathf.Max(distForHeight, distForWidth);
        float dist = Mathf.Max(minDistance, baseDist * distanceFactor);

        Vector3 camPos = center - playerForward * dist + playerRight * lateralOffset + Vector3.up * (height * 0.25f + verticalOffset);
        introCamTransform.position = camPos;
    }

    private Bounds GetPlayerBounds()
    {
        var renderers = playerRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }
        var colliders = playerRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                b.Encapsulate(colliders[i].bounds);
            return b;
        }
        // Fallback
        return new Bounds(playerRoot.position + Vector3.up, new Vector3(1.0f, 2.0f, 1.0f));
    }

    private Vector3 GetPlayerCenterWorld()
    {
        return GetPlayerBounds().center;
    }

    private IEnumerator IntroSequence()
    {
        playing = true;

        if (holdFrontDuration > 0f)
            yield return new WaitForSeconds(holdFrontDuration);

        if (shakeDuration > 0f && shakeAmplitude > 0f && shakeFrequency > 0f)
            yield return StartCoroutine(DoCameraShake(shakeDuration, shakeAmplitude, shakeFrequency, shakeFalloff));

        blendStartPos = introCamTransform.position;
        blendStartRot = introCamTransform.rotation;

        Vector3 blendTargetPos = mainCamera.transform.position;
        Quaternion blendTargetRot = mainCamera.transform.rotation;

        float t = 0f;
        while (t < blendDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / blendDuration);
            float e = blendCurve != null ? blendCurve.Evaluate(k) : k;

            introCamTransform.position = Vector3.Lerp(blendStartPos, blendTargetPos, e);
            introCamTransform.rotation = Quaternion.Slerp(blendStartRot, blendTargetRot, e);

            yield return null;
        }

        mainCamera.gameObject.SetActive(true);
        introCamera.gameObject.SetActive(false);

        // Spielerbewegung wieder freigeben
        if (lockPlayerMovementDuringIntro && playerBehaviour != null)
            playerBehaviour.enabled = true;

        playing = false;
    }

    private IEnumerator DoCameraShake(float duration, float amplitude, float frequency, AnimationCurve falloff)
    {
        Vector3 basePos = introCamTransform.position;
        Quaternion baseRot = introCamTransform.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float f = falloff != null ? falloff.Evaluate(k) : (1f - k);

            float nx = Mathf.PerlinNoise(0f, t * frequency) * 2f - 1f;
            float ny = Mathf.PerlinNoise(10f, t * frequency) * 2f - 1f;
            float nz = Mathf.PerlinNoise(20f, t * frequency) * 2f - 1f;

            Vector3 offsetPos = new Vector3(nx, ny, 0f) * (amplitude * f);
            float rotShake = nz * (amplitude * 20f * f);

            introCamTransform.position = basePos + introCamTransform.rotation * offsetPos;
            introCamTransform.rotation = baseRot * Quaternion.Euler(0f, rotShake, 0f);

            yield return null;
        }

        introCamTransform.position = basePos;
        introCamTransform.rotation = baseRot;
    }
}
