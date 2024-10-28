using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CombatGridSystem
{
    public static CombatGridSystem instance;
    public PathFinder PathGrid;
    public UnitPositionGrid UnitGrid;
    int width, height;
    Vector3 origin;
    float cellSize;

    public CombatGridSystem(int width, int height) : this(width, height, new Vector3(-Mathf.Round(width * 2f) / 4f, -Mathf.Round(width * 2f) / 4f)) {}

    public CombatGridSystem(int width, int height, Vector3 origin)
    {
        instance = this;
        this.origin = origin;

        this.width = width;
        this.height = height;

        PathGrid = new(width, height, origin);
        UnitGrid = new(width, height, origin);

        cellSize = PathGrid.GetGridSystem().getCellSize();

        GameObject DebugGrid = new("Debug_Grid");
        PathGrid.GetGridSystem().setDebugParent(DebugGrid.transform);
        PathGrid.GetGridSystem().drawDebugGrid();
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return (new Vector3(x, y) + origin) * cellSize;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - origin).x/ cellSize);
        y = Mathf.FloorToInt((worldPosition - origin).y/ cellSize);
    }

    public GameObject CreateWorldSquad(Squad squad, Controller controller, int x, int y)
    {
        GameObject WorldSquad = UtilityClass.CreatePrefabObject("Assets/PreFabs/Main Game/SquadEntity.prefab", controller.transform, squad.Name +"_World_Squad");
        WorldSquad.GetComponent<SpriteRenderer>().sprite = squad.RetrieveLeader().Icon;
        WorldSquad.SetActive(true);

        SquadMovementHandler smh = WorldSquad.GetComponent<SquadMovementHandler>();
        smh.AssignController(controller);
        smh.Deactivate();
        controller.SquadMoverList.Add(WorldSquad.GetComponent<SquadMovementHandler>());

        WorldSquad.transform.localPosition = GetWorldPosition(x, y) + Vector3.one * 0.5f;

        UnitGrid.InsertSquad(x, y, smh, squad);

        return WorldSquad;
    }

    public void LoadValidTiles(SquadMovementHandler smh)
    {
        Controller controller = smh.GetController();
        smh.ClearHighlighting();

        List<Vector3> ValidatedMoves = smh.GetValidatedPathList();
        List<Vector3> AttackVectors = smh.GetAttackVectors();

        UnitGrid.GetSquadCoordinate(smh, out int xPos, out int yPos);

        int MoveMax = smh.GetSquad().MoveSpeed + 1;

        //TODO: Check neighboring tiles to see if it has an enemy, if it does then mark it for attacking
        //TODO: Also when we find a neighboring tile with an enemy, apply Zone of Control

        bool diagonal = false;
        for(int x = xPos - MoveMax; x <= xPos + MoveMax; x++)
        {
            for(int y = yPos - MoveMax; y <= yPos + MoveMax; y++)
            {
                bool allyOccupied = false;
                foreach(SquadMovementHandler s in controller.SquadMoverList)
                {
                    UnitGrid.GetSquadCoordinate(s, out int smhX, out int smhY);

                    if(smhX == x && smhY == y) 
                    {
                        allyOccupied = true;
                        break;
                    }
                }

                if(x < 0 || y < 0 || x >= Level.MapSize.First || y >= Level.MapSize.Second || (x == xPos && y == yPos) || allyOccupied) continue;
                
                List<PathNode> path = PathGrid.FindPath(xPos, yPos, x, y, diagonal);
                if(path == null) continue;
                int pathCount = path.Count;
                bool walkable = PathGrid.IsPositionWalkable(x, y);
                bool hasPath = PathGrid.HasPath(xPos, yPos, x, y, diagonal);

                if(pathCount <= MoveMax && walkable && hasPath)
                {
                    Vector3 node = PathFinder.instance.GetGridSystem().GetWorldPosition(x, y);

                    GameObject HTile;

                    if(pathCount == MoveMax)
                    {
                        HTile = UtilityClass.CreatePrefabObject("Assets/PreFabs/Main Game/TileCombat.prefab", smh.transform, "Attack_Node_" + node);
                    }
                    else
                    {
                        ValidatedMoves.Add(node);
                        smh.AddValidatedCoordinate(new(x, y));
                        HTile = UtilityClass.CreatePrefabObject("Assets/PreFabs/Main Game/TileHighlight.prefab", smh.transform, "Move_Node_" + node);
                    }

                    AttackVectors.Add(node);
                    smh.AddAttackCoordinate(new(x, y));
                    HTile.transform.localPosition = smh.transform.InverseTransformPoint(node) + Vector3.one * 0.5f;
                    smh.AddHighlightedTile(HTile);
                }
            }
        }
    }

    public bool SetSquadNormalPath(SquadMovementHandler smh, Vector3 target)
    {
        UnitGrid.GetSquadCoordinate(smh, out int originX, out int originY);
        GetXY(target, out int targetX, out int targetY);
        smh.SetPathList(target);

        UnitGrid.MoveSquad(originX, originY, targetX, targetY);

        smh.StartMovementCallback();

        return true;
    }

    public bool SetSquadAttackPath(SquadMovementHandler attacking, SquadMovementHandler attacked)
    {
        UnitGrid.GetSquadCoordinate(attacked, out int attackX, out int attackY);
        UnitGrid.GetSquadCoordinate(attacked, out Pair<int, int> AttackedCoordinate);

        bool InCoordinate = false;
        InCoordinate = attacking.GetAttackCoordinates().Any((coordinate) => {
            return coordinate.equals(AttackedCoordinate);
        });
        
        if(!InCoordinate) return false;

        Pair<int, int> NeighborTile = new(0, 0);

        switch(attacked.EntrySide)
        {
            case SquadMovementHandler.Side.left:
                NeighborTile = new(attackX - 1, attackY - 0);
                break;
            
            case SquadMovementHandler.Side.right:
                NeighborTile = new(attackX + 1, attackY + 0);
                break;

            case SquadMovementHandler.Side.top:
                NeighborTile = new(attackX - 0, attackY + 1);
                break;

            case SquadMovementHandler.Side.bottom:
                NeighborTile = new(attackX + 0, attackY - 1);
                break;

            case SquadMovementHandler.Side.none:
                Debug.LogError("The controller cannot determine which tile to attack from\nController = " + attacking.GetController().ToString());
                return false;
        }

        if(NeighborTile.First > width || NeighborTile.First < 0
        || NeighborTile.Second > height || NeighborTile.Second < 0)
        {
            Debug.LogError("The controller tried to attack outside the grid bounds\nController = " + attacking.GetController().ToString());
            return false;
        }

        //Check if the NeighborTile is within the AttackVector bounds
        InCoordinate = false;
        Pair<int, int> NeighborCoordinate = new(NeighborTile.First, NeighborTile.Second);
        InCoordinate = attacking.GetAttackCoordinates().Any((coordinate) => {
            return coordinate.equals(NeighborCoordinate);
        });

        if(!InCoordinate)
        {
            Debug.LogError("The controller tried to attack outside of the Attack Vector bounds" +
            "\nController = " + attacking.GetController().ToString() +
            "Entry Side = " + attacking.EntrySide.ToString());
            return false;
        }

        //Check if the tile is already occupied
        if(UnitGrid.GetGridSystem().GetGridObject(NeighborTile.First, NeighborTile.Second) != null)
        {
            Debug.LogError("This tile is already occupied");
            return false;
        }

        SetSquadNormalPath(attacking, GetWorldPosition(NeighborTile.First, NeighborTile.Second));

        attacked.DeleteSquadCallback += () => {
            UnitGrid.RemoveSquad(attacked);
            Controller.selectedSquad = null;
            Controller.attackedSquad = null;
        };

        attacked.StartDeletionCallback(attacking);

        return true;
    }
}