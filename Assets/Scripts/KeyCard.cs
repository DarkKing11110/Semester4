using UnityEngine;

public class KeyCard : MonoBehaviour
{
    public Keypad2 Keypad2;

   
    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKey(KeyCode.E))
        {
            Keypad2.Keycard = true;
            GameEvents.OnMinigameCompleted?.Invoke("foundKeycard");
            //Play a sound of collecting the card
        }
    }
}
