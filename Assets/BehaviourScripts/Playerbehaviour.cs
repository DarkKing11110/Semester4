using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float mouseSensitivityYaw = 2f;   // Drehgeschwindigkeit horizontal
    public float mouseSensitivityPitch = 2f; // Drehgeschwindigkeit vertikal
    private float cameraPitch = 0f;          // Aktueller Pitch-Winkel der Kamera
    public float maxPitch = 90f;             // Maximale Neigung nach oben/unten
    public float minPitch = -90f;            // Minimale Neigung nach unten
    public float moveSpeed = 5f; // Bewegungsgeschwindigkeit des Spielers
    //public float sideMoveSpeed = 3f; // Seitliche Bewegungsgeschwindigkeit des Spielers
    public float attackRange = 2f; // Angriffsreichweite des Spielers
    public int attackDamage = 10; // Angriffsschaden des Spielers
    public Transform cameraTransform; // Referenz zur Kamera
    public float CameraHeight = 1.7f; // Höhe der Kamera über dem Boden
    public bool isRunning = false; // Flag, ob der Spieler rennt

    public float maxStamina = 100;
    public float currentStamina;
    public float staminaRegenRate = 15f; // Punkte pro Sekunde
    public float sprintMultiplier = 1.7f; // Sprint-Geschwindigkeit
    public float staminaDrainRate = 30f; // Punkte pro Sekunde beim Sprinten
    private bool canSprint => currentStamina > 0;

    public int maxHealth = 100;
    public int currentHealth;


    private Rigidbody rb;
    private bool isGrounded;
    private Texture2D healthTexture;
    private Texture2D staminaTexture;

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

        // Erzeuge eine 1x1 Texture für die Health-Bar-Farbe
        healthTexture = new Texture2D(1, 1);
        staminaTexture = new Texture2D(1, 1);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerDied();
        }
    }


    void Update()
    {
        HandleSprint();
        Move();
        Attack();
        HandleMouseLook();
        RegenerateStamina();
    }

    void HandleSprint()
    {
        // Sprinten mit linker Shift-Taste
        if (Input.GetKey(KeyCode.LeftShift) && canSprint)
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
            // Setze die Kamera-Rotation auf Pitch und Yaw
            cameraTransform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y, 0);
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
        if (isRunning && canSprint && (moveVertical != 0 || moveHorizontal != 0))
            speed *= sprintMultiplier;

        Vector3 movement = (camForward * moveVertical + camRight * moveHorizontal).normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
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
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 headOffset = new Vector3(0, CameraHeight, 0);
            cameraTransform.position = transform.position + headOffset;
            // Die Rotation wird in HandleMouseLook gesetzt
        }
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
        GUI.Label(new Rect(staminaX, staminaY, staminaBarWidth, staminaBarHeight), "Stamina", staminaStyle);
    }




}







