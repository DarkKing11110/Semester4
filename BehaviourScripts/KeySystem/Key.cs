using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyID = "DoorA"; // Eindeutige ID für die Tür
    public float viewRadius = 3f; // Radius, in dem der Schlüssel aufgenommen werden kann
    public float viewAngle = 30f; // Maximaler Winkel zur Blickrichtung des Spielers

    private bool playerCanPickUp = false;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        playerCanPickUp = false;

        if (player != null)
        {
            Vector3 playerEyePos = player.position + Vector3.up * 0.91f; // Augenhöhe ggf. anpassen
            float distance = Vector3.Distance(transform.position, playerEyePos);
            if (distance <= viewRadius)
            {
                Vector3 toKey = (transform.position - playerEyePos).normalized;
                Vector3 playerForward = player.forward;
                float angle = Vector3.Angle(playerForward, toKey);
                if (angle <= viewAngle)
                {
                    playerCanPickUp = true;
                }
            }
        }

        if (playerCanPickUp && Input.GetKeyDown(KeyCode.E))
        {
            PlayerKeyInventory inventory = player.GetComponent<PlayerKeyInventory>();
            if (inventory != null)
            {
                inventory.AddKey(keyID);
                Destroy(gameObject); // Schlüssel aufnehmen und entfernen
            }
        }
    }

    void OnGUI()
    {
        if (playerCanPickUp)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            float width = 200;
            float height = 40;
            float x = (Screen.width - width) / 2;
            float y = Screen.height * 0.8f;
            GUI.Label(new Rect(x, y, width, height), "Drücke E um Schlüssel zu nehmen", style);
        }
    }
}
