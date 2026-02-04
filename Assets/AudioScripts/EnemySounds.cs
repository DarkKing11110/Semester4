using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _footsteps;
    private FMOD.Studio.EventInstance footsteps;

    private EnemyMovementType currentAudiomovement = EnemyMovementType.Patrolling;

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

    public void SetEnemyMovementType(EnemyMovementType movement)
    {
        if (!footsteps.isValid())
        {
            return;
        }

        footsteps.setParameterByName("MovementType", (float)movement);
    }

    private EnemyMovementType currentAudioMovement = EnemyMovementType.Patrolling;

    public void UpdateMovementForAudio(EnemyMovementType movement)
    {
        

        if (movement == currentAudioMovement)
            return;

        currentAudioMovement = movement;


        if (footsteps.isValid())
            footsteps.setParameterByName("MovementType", (float)currentAudioMovement);
    }

}
