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
    void OnTriggerEnter(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Panel.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Panel.SetActive(false);
        }
    }


    //Testing Purposes
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Panel.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Panel.SetActive(false);
        }
    }
}