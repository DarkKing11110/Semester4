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
            return;
        }

        footsteps.setParameterByName("MovementType", (float)movement);
    }

    private MovementType currentAudioMovement = MovementType.Walk;


    public void UpdateMovementForAudio(bool isRunning, bool isCrouching)
    {
        MovementType newMovement;

        if (isCrouching)
            newMovement = MovementType.Crouch;
        else if (isRunning)
            newMovement = MovementType.Run;
        else
            newMovement = MovementType.Walk;

        // Only update FMOD if the state actually changed
        if (newMovement == currentAudioMovement)
            return;

        currentAudioMovement = newMovement;

        
        if (footsteps.isValid())
            footsteps.setParameterByName("MovementType", (float)currentAudioMovement);
    }

}
