using UnityEngine;
using  FMODUnity;
public class Keypad2 : MonoBehaviour
{
    public bool Keycard = false;
    private void OnTriggerStay(Collider other)
    {
        if (Keycard == true && Input.GetKey(KeyCode.E))
        {
            RuntimeManager.PlayOneShot("event:/Environment/Minigames/Granted");
            //Playsound to show the player used the keycard

        }
        else
        {
            RuntimeManager.PlayOneShot("event:/Environment/Minigames/Denied");
            //Play a sound for wrong
        }
    }
}
