using System;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    [SerializeField] int TeamNumber = 1;
    List<SquadMovementHandler> IdleSquads = new();
    List<SquadMovementHandler> DefensiveSquads = new(); //Essentially, they were already placed and cannot move unless threatened
    AI Processor = new EnemyAI();
    public static Vector3 InitialPlayerViewport;
    public static float InitialOrthographicSize;
    bool PickSemaphore = false;

    Timer WaitToPick;

    private void Start()
    {
        //Cycle through and assign Coordinates to pre-existing units
        /* foreach(SquadMovementHandler squad in SquadMoverList) squad.SetCurrentCoordinate(); */

        TeamNumbers.Add(this, TeamNumber);

        WaitToPick = new(2f, true, () => {
            Debug.Log("Posting semaphore");
            PickSemaphore = true;
            WaitToPick.reset();
        });
    }

    public override void ControllerLogic()
    {
        WaitToPick.update();

        foreach(SquadMovementHandler smh in SquadMoverList) smh.ClearHighlighting();

        if(selectedSquad != null)
        {
            Vector3 selectedSquadPosition = UtilityClass.CopyVector(selectedSquad.transform.position);
            Camera.main.orthographicSize = 5f;
            selectedSquadPosition.z = -10f;
            Camera.main.transform.position = selectedSquadPosition;

            Camera.main.transform.GetComponent<CameraController>().ClampFollow(selectedSquadPosition);
            
            if(selectedSquad.moving) return;
        }

        if(!PickSemaphore && SquadMoverList.Count > 0) return;

        PickSemaphore = false;

        System.Random random = new();

        IdleSquads.Clear();

        foreach(SquadMovementHandler squad in SquadMoverList)
        {
            if(!squad.moved) IdleSquads.Add(squad);
        }

        if(IdleSquads.Count == 0)
        {
            Finished = true;
            attackedSquad = null;
            selectedSquad = null;
            Camera.main.transform.position = InitialPlayerViewport;
            Camera.main.orthographicSize = InitialOrthographicSize;
            return;
        }

        selectedSquad = IdleSquads[random.Next(IdleSquads.Count)];

        CombatGridSystem.instance.LoadValidTiles(selectedSquad);

        //Check if there is a player squad nearby and destroy it
        foreach(Vector3 tile in selectedSquad.GetAttackVectors())
        {
            CombatGridSystem.instance.PathGrid.GetGridSystem().GetXY(tile, out int x, out int y);

            Pair<int, int> CurrentTile = new(x, y);

            foreach(Controller controller in TurnManager.instance.controllers)
            {
                if(TeamNumbers[controller] == TeamNumbers[this]) continue;

                foreach(SquadMovementHandler current in controller.SquadMoverList)
                {
                    if(controller.SquadMoverList.Count == 0) break;

                    Debug.Log("Controller squadlist size = " + controller.SquadMoverList.Count);

                    CombatGridSystem.instance.UnitGrid.GetSquadCoordinate(current, out var CurrentPair);

                    if(CurrentTile.equals(CurrentPair))
                    {
                        attackedSquad = current;
                        Debug.Log("Found squad to attack: " + attackedSquad.name);
                        break;
                    }
                }
            }

            if(attackedSquad != null) break;
        }
        
        if(attackedSquad != null) 
        {
            //TODO: Select entry side based on good entry parameters
            Array values = Enum.GetValues(typeof(SquadMovementHandler.Side));
            int idx = random.Next(values.Length);
            attackedSquad.EntrySide = (SquadMovementHandler.Side) values.GetValue(idx);
            if(!CombatGridSystem.instance.SetSquadAttackPath(selectedSquad, attackedSquad)) PickSemaphore = true;
            attackedSquad.EntrySide = SquadMovementHandler.Side.none;
        }
        else CombatGridSystem.instance.SetSquadNormalPath(selectedSquad, selectedSquad.GetValidatedPathList()[random.Next(selectedSquad.GetValidatedPathList().Count)]);
    }

    public enum Operation
    {
        Random,
        Aggressive,
        Defensive,
    }
}