using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnManager : MonoBehaviour 
{
    public static TurnManager instance;
    public static Level CurrentLevel;
    [SerializeField] public List<Controller> controllers;
    [SerializeField] public Stack<Pair<Vector3, Vector3>> MoveHistory = new();
    private protected Dictionary<Controller, Controller.Relationship> diplomacy = new();
    private protected int ControllerTurn = 0;
    private protected int rounds = 0;

    void Awake()
    {
        instance = this;
        
        CurrentLevel = new Tutorial();

        //Get children controllers
        controllers.AddRange(transform.GetComponentsInChildren<Controller>());

        SpawnTile[] spawners = transform.GetComponentsInChildren<SpawnTile>();

        foreach (SpawnTile spawner in spawners) controllers[spawner.ControllerNum].spawners.Add(spawner);
    }

    private void Update()
    {
        controllers[ControllerTurn].Update();
        controllers[ControllerTurn].ControllerLogic();

        if(controllers[ControllerTurn].Finished) 
        {
            MoveHistory.Clear();
            controllers[ControllerTurn].Finished = false;
            controllers[ControllerTurn].SquadMoverList.ForEach(squad => squad.Reactivate());
            controllers[ControllerTurn].TurnNumber++;
            ControllerTurn = (ControllerTurn + 1) % controllers.Count;
        }
    }
}