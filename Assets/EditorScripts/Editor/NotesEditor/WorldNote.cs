using UnityEngine;

public class WorldNote : MonoBehaviour
{
    [TextArea(3, 10)]
    public string noteText;
    public float viewRadius = 3f; // Radius, in dem die Notiz gelesen werden kann
    public float viewAngle = 30f; // Maximaler Winkel zur Blickrichtung des Spielers

    private bool playerCanRead = false;
    private bool showNote = false;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        playerCanRead = false;

        if (player != null)
        {
            // Augenhöhe des Spielers (ggf. anpassen)
            Vector3 playerEyePos = player.position + Vector3.up * 0.91f;

            float distance = Vector3.Distance(transform.position, playerEyePos);
            if (distance <= viewRadius)
            {
                Vector3 toNote = (transform.position - playerEyePos).normalized;
                Vector3 playerForward = player.forward;

                float angle = Vector3.Angle(playerForward, toNote);
                if (angle <= viewAngle)
                {
                    playerCanRead = true;
                }
            }
        }

        if (playerCanRead && Input.GetKeyDown(KeyCode.E))
        {
            showNote = !showNote;
        }
        else if (!playerCanRead)
        {
            showNote = false;
        }
    }


    void OnGUI()
    {
        if (playerCanRead && showNote)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 20,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            float width = 400;
            float height = 200;
            float x = (Screen.width - width) / 2;
            float y = Screen.height * 0.7f;
            GUI.Box(new Rect(x, y, width, height), noteText, style);
        }
        else if (playerCanRead && !showNote)
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
            GUI.Label(new Rect(x, y, width, height), "Drücke E für Notiz", style);
        }
    }
}
