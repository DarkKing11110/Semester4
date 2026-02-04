using UnityEngine;
using UnityEngine.Events;

public class ActivationTrigger : MonoBehaviour
{
    [SerializeField] string activationTag;
    [SerializeField] UnityEvent activationMethods;
    [SerializeField] UnityEvent deactivationMethods;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag(activationTag))
        //{
            
        //}
        activationMethods?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        deactivationMethods?.Invoke();
    }
}
