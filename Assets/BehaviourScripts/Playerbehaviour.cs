using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

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
    [Tooltip("Glättung der Kameradrehung (Yaw) in Third-Person.")]
    public float thirdPersonYawSmoothTime = 0.12f;

    [Header("Look")]
    public float mouseSensitivityYaw = 2f;
    public float mouseSensitivityPitch = 2f;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f; // Entkoppelter Kamera-Yaw (Ziel)
    private float cameraYawSmoothed = 0f; // Tatsächlich verwendeter geglätteter Yaw
    private float cameraYawVelocity = 0f; // interne Yaw-Geschwindigkeit für SmoothDampAngle
    public float maxPitch = 90f;
    public float minPitch = -90f;            // Minimale Neigung nach unten

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.7f;
    [Tooltip("Multiplikator für die Bewegungsgeschwindigkeit im Crouch (z.B. 0.6 = 60%).")]
    public float crouchSpeedMultiplier = 0.6f;
    [Tooltip("Wie schnell der Charakter zur Bewegungsrichtung dreht (Grad/Sekunde).")]
    public float turnSpeed = 540f;
    public bool isWalking;
    public bool isJumping;
    public bool isCrouching;
    public bool isWalkingBackwards;

    // NEU: Strafe-Flags
    [Header("Strafing")]
    public bool isStrafeForward;
    public bool isStrafeBackward;
    public bool isStrafeLeft;
    public bool isStrafeRight;

    [Header("Combat")]
    public float attackRange = 2f; // Angriffsreichweite des Spielers
    public int attackDamage = 10; // Angriffsschaden des Spiels
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

    // Audioshit
    

    // Felder
    [Tooltip("Dreht die Figur automatisch in die Bewegungsrichtung (Third-Person).")]
    public bool autoRotateToMoveDirection = false;

    [Tooltip("Max. Rotationswinkel pro Schritt in den Rückwärtsgang relativ zur aktuellen Blickrichtung (Grad).")]
    public float backwardRotateClamp = 45f;

    [SerializeField]
    private PlayerSounds playerSounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Nur X/Z Rotation sperren (Umkippen verhindern), Y erlauben für sauberes Drehen
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        healthTexture = new Texture2D(1, 1);
        staminaTexture = new Texture2D(1, 1);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            standColliderHeight = capsule.height;
            standColliderCenter = capsule.center;
            capsuleCached = true;
            float minHeight = Mathf.Max(0.01f, capsule.radius * 2f + 0.01f);
            if (crouchColliderHeight < minHeight) crouchColliderHeight = minHeight;
        }
        else
        {
            Debug.LogWarning("[PlayerBehaviour] Kein CapsuleCollider gefunden. Crouch-Kollisionsanpassung ist deaktiviert.");
        }

        if (useThirdPerson)
        {
            cameraYaw = transform.eulerAngles.y;
            cameraYawSmoothed = cameraYaw;
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
        //Attack();
        HandleMouseLook();
        RegenerateStamina();
        HandleAnimations();   // zentral wie der Sprint-Handler

         // Audio
        playerSounds.UpdateMovementForAudio(isRunning, isCrouching);
        UpdateBreathing();

    }

    void Move()
    {
        float hRaw = Input.GetAxis("Horizontal");
        float vRaw = Input.GetAxis("Vertical");

        float vPreferred = vRaw;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vPreferred = 1f; // W+S -> vorwärts bevorzugen

        // Kamera-Basis
        Quaternion camYawRotInstant = Quaternion.Euler(0f, cameraYaw, 0f);
        Vector3 camForward = camYawRotInstant * Vector3.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight   = camYawRotInstant * Vector3.right;   camRight.y   = 0f; camRight.Normalize();

        // Exakte Diagonalen für S+A / S+D (unten links / unten rechts)
        bool sComboLeft  = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W);
        bool sComboRight = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W);

        Vector3 inputDir;
        if (sComboLeft)
            inputDir = (-camForward - camRight);
        else if (sComboRight)
            inputDir = (-camForward + camRight);
        else
            inputDir = (camForward * vPreferred + camRight * hRaw);

        inputDir.y = 0f;

        // Reiner Rückwärtsgang nur bei "S" ohne Diagonale
        bool isBackwardPure = (vPreferred < -0.01f) && !sComboLeft && !sComboRight;

        // Spieler in Bewegungsrichtung drehen
        if (useThirdPerson && autoRotateToMoveDirection && inputDir.sqrMagnitude > 0.0001f)
        {
            Vector3 currentFwd = transform.forward; currentFwd.y = 0f; currentFwd.Normalize();
            Vector3 desiredFwd = inputDir.normalized;

            if (isBackwardPure)
            {
                // Nur beim reinen Rückwärtsgang Winkel begrenzen
                float angleDelta = Vector3.SignedAngle(currentFwd, desiredFwd, Vector3.up);
                float clampedDelta = Mathf.Clamp(angleDelta, -backwardRotateClamp, backwardRotateClamp);
                desiredFwd = Quaternion.AngleAxis(clampedDelta, Vector3.up) * currentFwd;
            }

            Quaternion targetRot = Quaternion.LookRotation(desiredFwd, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        float speed = moveSpeed;
        if (isRunning && canSprint) speed *= sprintMultiplier;
        if (isCrouching) speed *= Mathf.Clamp(crouchSpeedMultiplier, 0.1f, 1f);

        rb.MovePosition(transform.position + inputDir.normalized * speed * Time.deltaTime);
    }

    void HandleSprint()
    {
        float hRaw = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");

        float vPreferred = vRaw;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vPreferred = 1f;

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        Quaternion camYawRotInstant = Quaternion.Euler(0f, cameraYaw, 0f);
        Vector3 camFwd = camYawRotInstant * Vector3.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = camYawRotInstant * Vector3.right;  camRight.y = 0f; camRight.Normalize();

        bool sComboLeft  = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W);
        bool sComboRight = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W);

        Vector3 inputDir;
        if (sComboLeft)
            inputDir = (-camFwd - camRight);
        else if (sComboRight)
            inputDir = (-camFwd + camRight);
        else
            inputDir = (camFwd * vPreferred + camRight * hRaw);

        inputDir.y = 0f;

        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        float forwardComp = inputDir.sqrMagnitude > 0.0001f ? Vector3.Dot(inputDir.normalized, fwd) : 0f;

        const float sprintDirThreshold = 0.45f;
        bool wantsDirectionalRun = shiftHeld && forwardComp > sprintDirThreshold && canSprint;

        if (isCrouching && wantsDirectionalRun)
        {
            if (!preventStandUpIfObstructed || HasHeadroomToStand())
                SetCrouch(false);
        }

        if (!isCrouching && wantsDirectionalRun)
        {
            isRunning = true;
            currentStamina = Mathf.Clamp(currentStamina - staminaDrainRate * Time.deltaTime, 0f, maxStamina);
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

        // Pitch nur über Maus
        cameraPitch = Mathf.Clamp(cameraPitch - mouseY, minPitch, maxPitch);

        if (useThirdPerson)
        {
            // Third-Person: Kamera-Yaw nur über Maus; Spielerrotation NICHT hier verändern
            cameraYaw += mouseX;
        }
        else
        {
            // First-Person: Spieler-Yaw über Maus
            transform.Rotate(Vector3.up * mouseX);

            // Kamera-Rotation direkt setzen (FP)
            if (cameraTransform != null)
                cameraTransform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0);
        }
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    // Zentral: Kamera-Position/-Rotation berechnen
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

        // NEU: Yaw weich in Richtung des Ziel-Yaw bewegen, um seitliche Ausschläge zu dämpfen
        cameraYawSmoothed = Mathf.SmoothDampAngle(cameraYawSmoothed, cameraYaw, ref cameraYawVelocity, thirdPersonYawSmoothTime);

        Quaternion camRot = Quaternion.Euler(cameraPitch, cameraYawSmoothed, 0f);
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

        // Sanftes Nachziehen der Position (bereits vorhanden)
        if (cameraSmoothTime > 0f)
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref cameraSmoothVelocity, cameraSmoothTime);
        else
            cameraTransform.position = targetPos;

        cameraTransform.rotation = camRot;
    }

    //void Attack()
    //{
    //    if (Input.GetMouseButtonDown(1)) // Rechtsklick
    //    {
    //        RaycastHit hit;
    //        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
    //        {
    //            EnemyBehaviour enemy = hit.collider.GetComponent<EnemyBehaviour>();
    //            if (enemy != null)
    //            {
    //                // Animation wird im Handler gesteuert; hier nur Gameplay
    //                enemy.TakeDamage(attackDamage);
    //            }
    //        }
    //    }
    //}

    // Zentraler Animations-Handler (ohne Hashes, ohne Timer)
    void HandleAnimations()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");

        float vPreferred = vRaw;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vPreferred = 1f;

        bool hasMoveInput = new Vector2(h, vPreferred).sqrMagnitude > 0.0001f;

        Quaternion camYawRotInstant = Quaternion.Euler(0f, cameraYaw, 0f);
        Vector3 camFwd = camYawRotInstant * Vector3.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = camYawRotInstant * Vector3.right;  camRight.y = 0f; camRight.Normalize();

        bool sComboLeft  = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W);
        bool sComboRight = Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W);

        Vector3 inputDir;
        if (sComboLeft)
            inputDir = (-camFwd - camRight);
        else if (sComboRight)
            inputDir = (-camFwd + camRight);
        else
            inputDir = (camFwd * vPreferred + camRight * h);

        inputDir.y = 0f;

        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();

        float forwardComp = inputDir.sqrMagnitude > 0.0001f ? Vector3.Dot(inputDir.normalized, fwd) : 0f;
        float rightComp   = inputDir.sqrMagnitude > 0.0001f ? Vector3.Dot(inputDir.normalized, right) : 0f;

        const float dirThreshold = 0.35f;

        bool forwardFlag   = hasMoveInput && forwardComp >  dirThreshold;
        bool backwardFlag  = hasMoveInput && forwardComp < -dirThreshold;
        bool rightStrafe   = hasMoveInput && rightComp   >  dirThreshold;
        bool leftStrafe    = hasMoveInput && rightComp   < -dirThreshold;

        // S+A/S+D: Vorwärts-Anim + seitliche Strafe, Rückwärtsflag unterdrücken
        if (sComboLeft || sComboRight)
        {
            forwardFlag  = true;
            backwardFlag = false;
            leftStrafe   = sComboLeft;
            rightStrafe  = sComboRight;
        }

        isWalkingBackwards = (vPreferred < -0.01f) && hasMoveInput && !(sComboLeft || sComboRight);

        if (isDead)
        {
            isRunning = false;
            isWalking = false;
            isJumping = false;
            isAttacking = false;
            isAttacked = false;
            isCrouching = false;
            isWalkingBackwards = false;

            SetIdleFlags(false, false);
            SetStrafeFlags(false, false, false, false);
        }
        else
        {
            isWalking = hasMoveInput && !isRunning && !isWalkingBackwards;

            isAttacking = Input.GetMouseButton(1);

            bool idleStanding = !hasMoveInput && !isCrouching;
            bool idleCrouching = !hasMoveInput && isCrouching;
            SetIdleFlags(idleStanding, idleCrouching);

            SetStrafeFlags(forwardFlag, backwardFlag, leftStrafe, rightStrafe);
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isAttacked", isAttacked);
            animator.SetBool("isDead", isDead);
            animator.SetBool("isCrouching", isCrouching);
            animator.SetBool("isWalkingBackwards", isWalkingBackwards);

            animator.SetBool("isStrafeForward",   isStrafeForward);
            animator.SetBool("isStrafeBackward",  isStrafeBackward);
            animator.SetBool("isStrafeLeft",      isStrafeLeft);
            animator.SetBool("isStrafeRight",     isStrafeRight);
        }

        if (isAttacked && !isDead)
            isAttacked = false;


       
    }

    private void SetIdleFlags(bool idleStanding, bool idleCrouching)
    {
        if (animator == null) return;
        animator.SetBool("isIdleStanding", idleStanding);
        animator.SetBool("isIdleCrouching", idleCrouching);
    }

    // NEU: Hilfsmethode für Strafe-Flags
    private void SetStrafeFlags(bool forward, bool backward, bool left, bool right)
    {
        isStrafeForward  = forward;
        isStrafeBackward = backward;
        isStrafeLeft     = left;
        isStrafeRight    = right;
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

    
    private void PlayFootstep()
    {
        playerSounds.PlayFootsteps();
    }

    void UpdateBreathing()
    {
        if (isRunning)
            playerSounds.SetBreathState(PlayerSounds.BreathState.Running);
        else if (currentStamina < 40f)
            playerSounds.SetBreathState(PlayerSounds.BreathState.Exhausted);
        else
            playerSounds.SetBreathState(PlayerSounds.BreathState.Calm);
    }
    

    
}

