using UnityEngine;
using System;

public static class GameEvents
{
    public static Action<string> OnMinigameCompleted;
    public static Action<string> OnItemFound;
}
