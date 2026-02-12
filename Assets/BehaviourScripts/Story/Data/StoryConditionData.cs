using UnityEngine;

public abstract class StoryConditionData : ScriptableObject
{
    public abstract StoryCondition CreateCondition();  
}