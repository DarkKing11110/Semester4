using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerSounds;

public class EnemySounds : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _footsteps;
    private FMOD.Studio.EventInstance footsteps;

    [SerializeField] private FMODUnity.EventReference _breathing;
    private FMOD.Studio.EventInstance breathing;

    private EnemyMovementType currentAudiomovement = EnemyMovementType.Walking;

    private void Awake()
    {

        if (!_breathing.IsNull)
        {
            breathing = RuntimeManager.CreateInstance(_breathing);

            breathing.start();
        }

        if (!_footsteps.IsNull)
        {
            footsteps = FMODUnity.RuntimeManager.CreateInstance(_footsteps);
        }
    }

    private void Update()
    {

        breathing.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

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

    private EnemyMovementType currentAudioMovement = EnemyMovementType.Walking;

    public void UpdateMovementForAudio(EnemyMovementType movement)
    {
        

        if (movement == currentAudioMovement)
            return;

        currentAudioMovement = movement;


        if (footsteps.isValid())
            footsteps.setParameterByName("MovementType", (float)currentAudioMovement);
    }

    private MonsterBreathState currentMonsterBreathState = MonsterBreathState.Walking;

    public void SetBreathState(MonsterBreathState newState)
    {
        if (!breathing.isValid())
            return;

        if (newState == currentMonsterBreathState)
            return;

        currentMonsterBreathState = newState;

        breathing.setParameterByName(
            "BreathState",
            (float)newState
        );
    }

    public enum MonsterBreathState
    {
        Walking = 0,
        Running = 1,
        Exhausted = 2,
    }

}
