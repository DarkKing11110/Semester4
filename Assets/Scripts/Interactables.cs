using UnityEngine;
using UnityEngine.UI;

public class Interactables : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject Panel;


    private void Start()
    {
        Panel.SetActive(false);
    }
    void OnTriggerStay(Collider other)
    {
        if (Input.GetKey(KeyCode.E)) 
        {
            print("Works");
            Panel.SetActive(true);
        }
        if (Input.GetKey(KeyCode.Escape)) 
        {
            Panel.SetActive(false);
        }
    }
}