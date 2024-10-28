using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitPositionGrid
{
    public static UnitPositionGrid instance { get; set; }

    private GridSystem<SquadMovementHandler> grid;
    private Vector3 origin;

    Dictionary<SquadMovementHandler, Pair<int, int>> PositionMap = new();

    public UnitPositionGrid(int width, int height) : this(width, height, new Vector3(-Mathf.Round(width * 2f) / 4f, -Mathf.Round(width * 2f) / 4f)) {}

    public UnitPositionGrid(int width, int height, Vector3 origin)
    {
        instance = this;
        this.origin = origin;
        grid = new GridSystem<SquadMovementHandler>(width, height, 1f, origin, (GridSystem<SquadMovementHandler> smh, int width, int height) => (null));

        grid.OnGridObjectChanged += (sender, args) => {
            if(args.value == null) return;
            if(PositionMap.ContainsKey(args.value))
            PositionMap[args.value] = new(args.x, args.y);
            else
            PositionMap.Add(args.value, new(args.x, args.y));
        };

        List<SquadMovementHandler> list = Object.FindObjectsOfType<SquadMovementHandler>().ToList(); //Can set to true, to find inactive objects

        foreach(SquadMovementHandler smh in list)
        {
            grid.GetXY(smh.transform.position, out int x, out int y);
            grid.SetGridObject(x, y, smh);
        }
    }

    public GridSystem<SquadMovementHandler> GetGridSystem()
    {
        return grid;
    }

    public void InsertSquad(int x, int y, SquadMovementHandler smh, Squad squad = default)
    {
        smh.AssignSquad(squad);
        grid.SetGridObject(x, y, smh);
    }

    public void InsertSquad(Pair<int, int> coordinate, SquadMovementHandler smh, Squad squad = default)
    {
        InsertSquad(coordinate.First, coordinate.Second, smh, squad);
    }

    public void MoveSquad(int x1, int y1, int x2, int y2)
    {
        SquadMovementHandler temp = grid.GetGridObject(x1, y1);
        if(temp == null)
        {
            Debug.LogError("Trying to move a null squad!");
            return;
        }
        grid.SetGridObject(x1, y1, null);
        grid.SetGridObject(x2, y2, temp);
    }
    
    public void MoveSquad(Pair<int, int> p1, Pair<int, int> p2)
    {
        MoveSquad(p1.First, p1.Second, p2.First, p2.Second);
    }

    public void GetSquadCoordinate(SquadMovementHandler smh, out int x, out int y)
    {
        Pair<int, int> pair = PositionMap[smh];
        x = pair.First;
        y = pair.Second;
    }

    public void GetSquadCoordinate(SquadMovementHandler smh, out Pair<int, int> coordinate)
    {
        GetSquadCoordinate(smh, out int x, out int y);
        coordinate = new(x, y);
    }

    public void RemoveSquad(SquadMovementHandler smh)
    {
        if(!PositionMap.ContainsKey(smh))
        {
            Debug.LogError("Squad Movement Handler Entity does not exist in this Grid System context.\n" +
            "Squad name = " + smh.name + "\nI cannot remove something that doesn't exist");
        }

        smh.GetController().SquadMoverList.Remove(smh);
        PositionMap.Remove(smh);
        Object.Destroy(smh.transform.gameObject);
    }
}