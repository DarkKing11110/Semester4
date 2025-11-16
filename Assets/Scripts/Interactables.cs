using UnityEngine;

public class Interactables : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Panel GamePanel;

    void OnTriggerEnter(collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GamePanel.Visible = True;
        }
        if (Input.GetKeyDown(KeyCode.Esc))
        {
            GamePanel.Visible = False;
        }
    } 
}    

