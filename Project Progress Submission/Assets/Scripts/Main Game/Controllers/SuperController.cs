using System.Collections.Generic;
using UnityEngine;

public class SuperController : MonoBehaviour
{
    public static SquadMovementHandler selectedSquad;
    [SerializeField] public List<SquadMovementHandler> squadList;

    public byte ControllerNumber;
    public static byte ControllerTurn = 0; //0 Is ALWAYS the player

    private protected SerializableDictionary<byte, Relationship> diplomacy = new(){
        {0, Relationship.controllable}
    };

    public void Update()
    {
        if(selectedSquad == null) return;

        if(diplomacy[ControllerTurn] == Relationship.controllable)
        {
            ControllerLogic();
        }
    }

    public virtual void ControllerLogic()
    {
        //Debug.Log("Controller logic goes here");
    }

    //TODO: Pass in file that contains a list of units and squads
    public virtual bool SetupSquadlist()
    {
        Debug.Log("Make sure to load all squads!");
        return false;
    }

    public enum Relationship
    {
        ally, //You cannot attack them, period. They will try to help.
        enemy, //You are free to attack them, and they are free to attack you.
        neutral, //They are potential targets, but attacking leads to them being enemies. They can also calculate to attack you or not.
        controllable //Your relationship to yourself. But, for funsies, could possibly implement something to where the Player Controller can control more than 1 "team"
    }
}