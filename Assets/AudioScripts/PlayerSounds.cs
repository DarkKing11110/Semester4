using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds: MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _footsteps;
    private FMOD.Studio.EventInstance footsteps;

    private void Awake()
    {
        if (!_footsteps.IsNull)
        {
            footsteps = FMODUnity.RuntimeManager.CreateInstance(_footsteps);
        }
    }

    public void PlayFootsteps()
    {
        if (footsteps.isValid())
        {
            footsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            footsteps.start();
        }
    }



    public void SetMovementType(MovementType movement)
    {
        if (!footsteps.isValid())
        { 
            Debug.LogWarning("Footsteps event is NOT valid");
            return;
        }

        Debug.Log($"Setting FMOD MovementType to: {movement} ({(float)movement})");

        footsteps.setParameterByName("MovementType", (float)movement);
    }

   
}
