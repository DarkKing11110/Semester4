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

    [SerializeField] LayerMask occlusionMask;
    [SerializeField] float rayOffset = 0.5f;
    float occlusionSmoothed;

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

       {
            float occ = CalculateOcclusion();

            occlusionSmoothed = Mathf.Lerp(
                occlusionSmoothed,
                occ,
                Time.deltaTime * 8f
            );
            
            Debug.Log("Occlusion: " + occ);

            breathing.setParameterByName("Occlusion", occlusionSmoothed);
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

    float CalculateOcclusion()
    {
        Vector3 listener = Camera.main.transform.position;
        Vector3 source = transform.position;

        int blocked = 0;

        Vector3[] offsets =
        {
        Vector3.zero,
        Vector3.up * rayOffset,
        Vector3.down * rayOffset,
        Vector3.right * rayOffset,
        Vector3.left * rayOffset
    };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 target = source + offsets[i];

            if (Physics.Linecast(listener, target, occlusionMask))
            {
                blocked++;
                Debug.DrawLine(listener, target, Color.red);
            }
            else
            {
                Debug.DrawLine(listener, target, Color.green);
            }
        }

        return blocked / (float)offsets.Length;
    }



}
