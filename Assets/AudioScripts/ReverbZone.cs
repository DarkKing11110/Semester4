using UnityEngine;
using FMODUnity;

public class ReverbZone : MonoBehaviour
{
    public ReverbEnvironment environment;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        float value = (float)environment;

        ReverbManager.Instance.SetEnvironment(value);

    }

    public enum ReverbEnvironment
    {
        SmallRoom = 0,
        Hallway = 1,
        BigRoom = 2
    }
}
