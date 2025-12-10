using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("View")]
    [Tooltip("Wenn aktiviert, nutzt der Spieler die Third-Person-Kamera.")]
    public bool useThirdPerson = false; // Tickbox im Inspector
    [Tooltip("Sanftes Nachziehen der Kamera (Third-Person). 0 = direkt.")]
    public float cameraSmoothTime = 0.05f;

    [Header("Third-Person Einstellungen")]
    [Tooltip("Abstand der Kamera hinter dem Spieler (Third-Person).")]
    public float thirdPersonDistance = 3.0f;
    [Tooltip("Seitlicher Versatz der Kamera (positive Werte = rechte Schulter).")]
    public float thirdPersonSideOffset = 0.5f;
    [Tooltip("Minimaler Abstand bei Kollisionen.")]
    public float thirdPersonMinDistance = 0.3f;
    [Tooltip("Puffer bei Kollisionen, um Clipping zu vermeiden.")]
    public float cameraCollisionBuffer = 0.1f;
    [Tooltip("Radius für SphereCast bei Kamerakollision.")]
    public float cameraCollisionRadius = 0.2f;
    [Tooltip("Layer, die Kamera-Kollisionen blockieren.")]
    public LayerMask cameraObstructionMask = ~0;

    [Header("Look")]
    public float mouseSensitivityYaw = 2f;   // Drehgeschwindigkeit horizontal
    public float mouseSensitivityPitch = 2f; // Drehgeschwindigkeit vertikal
    private float cameraPitch = 0f;          // Aktueller Pitch-Winkel der Kamera
    public float maxPitch = 90f;             // Maximale Neigung nach oben/unten
    public float minPitch = -90f;            // Minimale Neigung nach unten

    [Header("Movement")]
    public float moveSpeed = 5f; // Bewegungsgeschwindigkeit des Spielers
    public float sprintMultiplier = 1.7f; // Sprint-Geschwindigkeit
    [Tooltip("Multiplikator für die Bewegungsgeschwindigkeit im Crouch (z.B. 0.6 = 60%).")]
    public float crouchSpeedMultiplier = 0.6f;
    public bool isWalking;
    public bool isJumping;
    public bool isCrouching;              // Crouch-Status (Toggle)
    public bool isWalkingBackwards;       // Rückwärtslauf-Status

    [Header("Combat")]
    public float attackRange = 2f; // Angriffsreichweite des Spielers
    public int attackDamage = 10; // Angriffsschaden des Spielers
    public bool isAttacking;
    public bool isAttacked;
    public bool isDead;

    [Header("Camera")]
    public Transform cameraTransform; // Referenz zur Kamera
    public float CameraHeight = 1.7f; // Höhe der Kamera über dem Boden

    [Header("Sprint/Stamina")]
    public bool isRunning = false; // Flag, ob der Spieler rennt
    public float maxStamina = 100;
    public float currentStamina;
    public float staminaRegenRate = 15f; // Punkte pro Sekunde
    public float staminaDrainRate = 30f; // Punkte pro Sekunde beim Sprinten
    private bool canSprint => currentStamina > 0;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Animation")]
    [Tooltip("Animator am Spieler (wird automatisch gesucht, wenn leer).")]
    public Animator animator;

    [Header("Crouch (Collider)")]
    [Tooltip("Zielhöhe des CapsuleColliders im Ducken (lokale Einheiten).")]
    public float crouchColliderHeight = 1.0f;
    [Tooltip("Aufstehen verhindern, wenn oben kein Platz ist.")]
    public bool preventStandUpIfObstructed = true;
    [Tooltip("Layer, die das Aufstehen blockieren (z. B. Decken/Umgebung).")]
    public LayerMask standUpObstructionMask = ~0;

    private Rigidbody rb;
    private Texture2D healthTexture;
    private Texture2D staminaTexture;

    // interne Kamera-Interpolation
    private Vector3 cameraSmoothVelocity;

    // Collider/Crouch Caching
    private CapsuleCollider capsule;
    private float standColliderHeight;
    private Vector3 standColliderCenter;
    private bool capsuleCached;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Sperrt die Rotation um die X- und Z-Achse, um das Umkippen zu verhindern
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Mauszeiger verstecken und zentrieren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Erzeuge eine 1x1 Texture für die Health-/Stamina-Bar-Farbe
        healthTexture = new Texture2D(1, 1);
        staminaTexture = new Texture2D(1, 1);

        // Animator automatisch finden
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // CapsuleCollider cachen (für Crouch)
        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            standColliderHeight = capsule.height;
            standColliderCenter = capsule.center;
            capsuleCached = true;
            // Sicherheitsklausel: crouch-Höhe nicht kleiner als 2*Radius
            float minHeight = Mathf.Max(0.01f, capsule.radius * 2f + 0.01f);
            if (crouchColliderHeight < minHeight) crouchColliderHeight = minHeight;
        }
        else
        {
            Debug.LogWarning("[PlayerBehaviour] Kein CapsuleCollider gefunden. Crouch-Kollisionsanpassung ist deaktiviert.");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        isAttacked = true; // 1-Frame-Impulse; Reset erfolgt in HandleAnimations()
        if (currentHealth <= 0)
        {
            isDead = true;
            currentHealth = 0;
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerDied();
        }
    }

    void Update()
    {
        HandleSprint();
        HandleCrouch();       // Crouch (Toggle) + Collider-Handling
        Move();
        Attack();
        HandleMouseLook();
        RegenerateStamina();
        HandleAnimations();   // zentral wie der Sprint-Handler
    }

    void HandleSprint()
    {
        // Bewegungseingabe abfragen (rohe Achsen, kein Smoothing)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = (new Vector2(h, v).sqrMagnitude > 0.0001f);

        // Sprinten nur, wenn:
        // - Shift gedrückt
        // - Bewegungseingabe vorhanden
        // - Stamina > 0
        // - NICHT crouching
        if (!isCrouching && Input.GetKey(KeyCode.LeftShift) && hasMoveInput && canSprint)
        {
            isRunning = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
        else
        {
            isRunning = false;
        }
    }

    // Toggle-Eingabe, Collider-Resize und Headroom-Check
    void HandleCrouch()
    {
        if (isDead) { SetCrouch(false); return; }

        // Umschalten bei Tastendruck (nicht halten)
        if (Input.GetKeyDown(KeyCode.C))
        {
            bool wantCrouch = !isCrouching;

            // Wenn wir aufstehen möchten, optional Headroom prüfen
            if (!wantCrouch && preventStandUpIfObstructed)
            {
                if (!HasHeadroomToStand())
                {
                    wantCrouch = true; // oben blockiert -> geduckt bleiben
                }
            }

            if (wantCrouch != isCrouching)
            {
                SetCrouch(wantCrouch);
            }
        }
    }

    void SetCrouch(bool crouched)
    {
        isCrouching = crouched;

        if (!capsuleCached || capsule == null) return;

        float targetHeight = crouched ? crouchColliderHeight : standColliderHeight;
        // Mindesthöhe absichern
        float minHeight = Mathf.Max(0.01f, capsule.radius * 2f + 0.01f);
        targetHeight = Mathf.Max(targetHeight, minHeight);

        // Center so verschieben, dass die "Füße" am Boden bleiben.
        Vector3 localAxis = GetLocalAxisFromDirection(capsule.direction);
        Vector3 targetCenter = standColliderCenter + localAxis * ((targetHeight - standColliderHeight) * 0.5f);

        capsule.height = targetHeight;
        capsule.center = targetCenter;
    }

    bool HasHeadroomToStand()
    {
        if (!capsuleCached || capsule == null) return true;

        // Prüfe geplante Stand-Konfiguration im Raum
        Vector3 localAxis = GetLocalAxisFromDirection(capsule.direction);

        // Stand-Center lokal/welt
        Vector3 standCenterLocal = standColliderCenter;
        Vector3 standCenterWorld = transform.TransformPoint(standCenterLocal);

        Vector3 axisWorld = transform.TransformDirection(localAxis).normalized;

        float standHeight = standColliderHeight;
        float radius = capsule.radius;

        // Endkappen-Mittelpunkte
        float halfLine = Mathf.Max(0f, (standHeight * 0.5f) - radius);
        Vector3 p1 = standCenterWorld + axisWorld * halfLine;
        Vector3 p2 = standCenterWorld - axisWorld * halfLine;

        // Prüfen, ob diese Kapsel kollidiert
        bool blocked = Physics.CheckCapsule(p1, p2, radius, standUpObstructionMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    // Hilfsfunktion: lokale Achse aus CapsuleCollider.direction
    static Vector3 GetLocalAxisFromDirection(int direction)
    {
        switch (direction)
        {
            case 0: return Vector3.right;   // X
            case 1: return Vector3.up;      // Y
            case 2: return Vector3.forward; // Z
            default: return Vector3.up;
        }
    }

    void RegenerateStamina()
    {
        if (!isRunning && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityYaw;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityPitch;

        // Spieler horizontal drehen (Yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Kamera vertikal drehen (Pitch)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        
        if (cameraTransform != null)
        {
            // In First-Person die Kamera-Rotation direkt setzen.
            // In Third-Person wird die Rotation und Position in UpdateCameraPosition berechnet.
            if (!useThirdPerson)
            {
                cameraTransform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0);
            }
        }
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        float speed = moveSpeed;

        // Sprint nur bei tatsächlichem Input (bereits in HandleSprint geprüft), hier reicht Status
        if (isRunning && canSprint && (moveVertical != 0 || moveHorizontal != 0))
            speed *= sprintMultiplier;

        // Geschwindigkeit im Crouch reduzieren (Editor-Wert)
        if (isCrouching)
            speed *= Mathf.Clamp(crouchSpeedMultiplier, 0.1f, 1f);

        Vector3 movement = (camForward * moveVertical + camRight * moveHorizontal).normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);

        // isWalking wird zentral in HandleAnimations() abgeleitet
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(1)) // Rechtsklick
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                EnemyBehaviour enemy = hit.collider.GetComponent<EnemyBehaviour>();
                if (enemy != null)
                {
                    // Animation wird im Handler gesteuert; hier nur Gameplay
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
    }

    // Zentraler Animations-Handler (ohne Hashes, ohne Timer)
    void HandleAnimations()
    {
        // Bewegung aus Input ableiten (wie Sprint-Handler: pro Frame auslesen)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude > 0.0001f;

        // Rückwärtslaufen (v < 0 bedeutet Input nach hinten relativ zur Kameraausrichtung)
        isWalkingBackwards = v < -0.01f && hasMoveInput;

        if (isDead)
        {
            isRunning = false;
            isWalking = false;
            isJumping = false;
            isAttacking = false;
            isAttacked = false;
            isCrouching = false;
            isWalkingBackwards = false;

            // Idle-States bei Tod deaktivieren
            SetIdleFlags(false, false);
        }
        else
        {
            // „Gehen“: Bewegung vorhanden, nicht rennen
            isWalking = hasMoveInput && !isRunning;

            // Angriff so lange wie Maustaste gehalten wird
            isAttacking = Input.GetMouseButton(1);

            // Idle: keine Bewegungseingabe
            bool idleStanding = !hasMoveInput && !isCrouching;
            bool idleCrouching = !hasMoveInput && isCrouching;
            SetIdleFlags(idleStanding, idleCrouching);
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isAttacked", isAttacked);
            animator.SetBool("isDead", isDead);

            // zusätzliche Animations-Flags
            animator.SetBool("isCrouching", isCrouching);
            animator.SetBool("isWalkingBackwards", isWalkingBackwards);
            // Idle-Flags werden in SetIdleFlags gesetzt
        }

        // Einmal-Flag am Ende zurücksetzen, damit der Animator exakt 1 Frame „isAttacked“ erhält
        if (isAttacked && !isDead)
            isAttacked = false;
    }

    // Hilfsmethode: setzt Idle-Flags konsistent und exklusiv
    private void SetIdleFlags(bool idleStanding, bool idleCrouching)
    {
        if (animator == null) return;

        // Exklusivität: nur einer der Idle-States aktiv
        animator.SetBool("isIdleStanding", idleStanding);
        animator.SetBool("isIdleCrouching", idleCrouching);
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform == null) return;

        Vector3 headOffset = new Vector3(0, CameraHeight, 0);
        Vector3 pivot = transform.position + headOffset;

        if (!useThirdPerson)
        {
            // First-Person: Kamera auf Kopfposition setzen, Rotation kommt aus HandleMouseLook
            cameraTransform.position = pivot;
            return;
        }

        // Third-Person: gewünschte Kameraposition hinter/seitlich des Spielers
        Quaternion camRot = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0f);
        Vector3 desiredOffset = camRot * new Vector3(thirdPersonSideOffset, 0f, -thirdPersonDistance);
        Vector3 desiredPos = pivot + desiredOffset;

        // Kollisionsvermeidung (SphereCast vom Pivot zur Zielposition)
        Vector3 toCam = desiredPos - pivot;
        float desiredDist = toCam.magnitude;
        Vector3 dir = desiredDist > 0.0001f ? toCam / desiredDist : Vector3.back;

        float targetDist = desiredDist;
        if (Physics.SphereCast(pivot, cameraCollisionRadius, dir, out RaycastHit hit, desiredDist, cameraObstructionMask, QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Max(hit.distance - cameraCollisionBuffer, thirdPersonMinDistance);
        }

        Vector3 targetPos = pivot + dir * targetDist;

        // Sanftes Nachziehen
        if (cameraSmoothTime > 0f)
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref cameraSmoothVelocity, cameraSmoothTime);
        else
            cameraTransform.position = targetPos;

        // Kamera-Rotation entsprechend Maus (Yaw/Pitch), konsistent mit First-Person
        cameraTransform.rotation = camRot;
    }

    void OnGUI()
    {
        // Health Bar (unten links)
        float barWidth = 120f;
        float barHeight = 12f;
        float x = 20f;
        float y = Screen.height - barHeight - 20f;

        GUI.color = Color.white;
        GUI.Box(new Rect(x, y, barWidth, barHeight), "");

        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);
        float fillWidth = barWidth * healthPercent;
        Color fillColor = Color.Lerp(Color.red, new Color(0f, 0.5f, 0f), healthPercent);
        healthTexture.SetPixel(0, 0, fillColor);
        healthTexture.Apply();
        GUI.DrawTexture(new Rect(x, y, fillWidth, barHeight), healthTexture);

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(x, y, barWidth, barHeight), "Health", style);

        // Stamina Bar (unten rechts)
        float staminaBarWidth = 120f;
        float staminaBarHeight = 12f;
        float staminaX = Screen.width - staminaBarWidth - 20f;
        float staminaY = Screen.height - staminaBarHeight - 20f;

        GUI.color = Color.white;
        GUI.Box(new Rect(staminaX, staminaY, staminaBarWidth, staminaBarHeight), "");
        float staminaPercent = Mathf.Clamp01(currentStamina / maxStamina);
        float staminaFillWidth = staminaBarWidth * staminaPercent;
        Color staminaFillColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(0.1f, 0.6f, 0.1f), staminaPercent); // von grau zu dunklem grün
        staminaTexture.SetPixel(0, 0, staminaFillColor);
        staminaTexture.Apply();
        GUI.DrawTexture(new Rect(staminaX, staminaY, staminaFillWidth, staminaBarHeight), staminaTexture);

        GUIStyle staminaStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(staminaX, staminaY, barWidth, staminaBarHeight), "Stamina", staminaStyle);
    }
}

























