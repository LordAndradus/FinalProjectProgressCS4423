using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AI
{
    Stack<State> StateMachine = new();
    Dictionary<State, System.Action> StateMap = new();
    
    public enum Mode
    {
        MoveToPosition,
        Follow,
        Defend,
        CaptureObjective,
        KillCertainSquad,
    }

    public enum State
    {

    }
}