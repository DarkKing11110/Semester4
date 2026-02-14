using UnityEngine;

public class Keypad2 : MonoBehaviour
{
    public bool Keycard = false;
    private void OnTriggerStay(Collider other)
    {
        if (Keycard == true && Input.GetKey(KeyCode.E))
        {
            //Playsound to show the player used the keycard

        }
        else
        {
            //Play a sound for wrong
        }
    }
}
