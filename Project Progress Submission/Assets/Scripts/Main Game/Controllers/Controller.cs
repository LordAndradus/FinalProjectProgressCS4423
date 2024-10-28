using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Controller : MonoBehaviour
{
    public static SquadMovementHandler selectedSquad;
    public static SquadMovementHandler attackedSquad;
    [SerializeField] public int TurnNumber = 0;
    [SerializeField] public List<SquadMovementHandler> SquadMoverList = new();
    [SerializeField] public List<SpawnTile> spawners = new();
    [SerializeField] public GameObject spawnerParent;
    [SerializeField] private protected Relationship relation = Relationship.neutral;
    [SerializeField] private protected Camera FollowCamera;
    [SerializeField] private protected Camera MainCamera;
    
    private protected static Dictionary<Controller, int> TeamNumbers = new();
    private protected static Dictionary<Controller, Relationship> RelationshipMap = new();

    public bool Finished = false;

    bool Awakened = false;

    public void Awake()
    {
        if(Awakened)
        {
            Debug.LogError("Thic controller is somehow re-awakened!");
            return;
        }

        TeamNumbers.Clear();
        RelationshipMap.Clear();

        foreach(Transform child in transform)
        {
            SquadMovementHandler smh = child.GetComponent<SquadMovementHandler>();
            smh.AssignController(this);
            SquadMoverList.Add(smh);
        }
        
        Awakened = true;
    }

    public void Update()
    {
        if(selectedSquad == null) return;
    }

    public Relationship GetOpposingRelationship(SquadMovementHandler smh)
    {
        return GetSquadController(smh).GetRelationship();
    }

    public Controller GetSquadController(SquadMovementHandler smh)
    {
        return smh.RetrieveController();
    }

    public Relationship GetRelationship()
    {
        return relation;
    }

    public virtual void ControllerLogic()
    {
        Debug.Log("Controller logic goes here");
    }

    //TODO: Pass in file that contains a list of units and squads
    public virtual bool SetupSquadlist()
    {
        Debug.Log("Make sure to load all squads!");
        return false;
    }

    public int GetTeamNumber(Controller controller = null)
    {
        if(controller == null) return TeamNumbers[this];
        return TeamNumbers[controller];
    }

    public enum Relationship
    {
        ally, //You cannot attack them, period. They will try to help.
        enemy, //You are free to attack them, and they are free to attack you.
        neutral, //They are potential targets, but attacking leads to them being enemies. They can also calculate to attack you or not.
        controllable //Your relationship to yourself. But, for funsies, could possibly implement something to where the Player Controller can control more than 1 "team"
    }
}