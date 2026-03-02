using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds: MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _footsteps;
    private FMOD.Studio.EventInstance footsteps;
    [SerializeField] private float footstepCooldown = 0.3f;
    private float lastFootstepTime;

    [SerializeField] private FMODUnity.EventReference _breathing;
    private FMOD.Studio.EventInstance breathing;

   


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

        storyManager = FindFirstObjectByType<StoryManager>();
    }

    private void Update()
    {
  
            breathing.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        {
            //  Assign 3D attributes
            currentVoice.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        }

        if (storyManager == null)
            return;

        if (storyManager.CurrentStepIndex != lastStepIndex)
        {
            lastStepIndex = storyManager.CurrentStepIndex;
            HandleStoryStep(lastStepIndex);
        }
    }

    public void PlayFootsteps()
    {
        if (footsteps.isValid())
        {
            if (Time.time - lastFootstepTime < footstepCooldown)
                return;

            lastFootstepTime = Time.time;

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

    private BreathState currentBreathState = BreathState.Calm;

    public void SetBreathState(BreathState newState)
    {
        if (!breathing.isValid())
            return;

        if (newState == currentBreathState)
            return;

        currentBreathState = newState;

        breathing.setParameterByName(
            "BreathState",
            (float)newState
        );
    }


    public enum BreathState
    {
        Calm = 0,
        Recover = 1,
        Exhausted = 2,
        Hiding = 3,
        Panic = 4,
        Running = 5
    }
    public enum MovementType
    {
        Walk = 0,
        Run = 1,
        Crouch = 2
    }



    private StoryManager storyManager;
    private int lastStepIndex = -1;

    [System.Serializable]
    public class StepVoiceMapping
    {
        public int stepIndex;
        public FMODUnity.EventReference voiceEvent;
    }

    [SerializeField] private List<StepVoiceMapping> stepVoices;

    private void HandleStoryStep(int index)
    {
        foreach (var mapping in stepVoices)
        {
            if (mapping.stepIndex == index)
            {
                PlayStoryVoice(mapping.voiceEvent);
                return;
            }
        }
    }

    private FMOD.Studio.EventInstance currentVoice;

    public void PlayStoryVoice(FMODUnity.EventReference voiceEvent)
    {
        if (voiceEvent.IsNull)
        {
            Debug.LogError("[VOICE] Voice event is NULL.");
            return;
        }

        // Stop previous voice if active
        if (currentVoice.isValid())
        {
            Debug.Log("[VOICE] Stopping previous voice.");
            currentVoice.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        currentVoice = FMODUnity.RuntimeManager.CreateInstance(voiceEvent);

        if (!currentVoice.isValid())
        {
            Debug.LogError("[VOICE] Failed to create instance.");
            return;
        }
    

        Debug.Log("[VOICE] 3D attributes set at position: " + transform.position);

        currentVoice.start();
        currentVoice.release();

        Debug.Log("[VOICE] Voice started successfully.");
    }
}

