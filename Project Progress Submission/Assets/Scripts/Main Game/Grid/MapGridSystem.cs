using UnityEngine;

public class MapGridSystem
{
    public static MapGridSystem instance;

    private PathFinder pf;
    private UnitPositionGrid upg;

    public MapGridSystem(int width, int height) : this (width, height, new Vector3(-Mathf.Round(width/2f) * 2f,  -Mathf.Round(width/2f) * 2f)) {}

    public MapGridSystem(int width, int height, Vector3 origin)
    {
        instance = this;
        pf = new(width, height, origin);
        upg = new(width, height, origin);

        pf.GetGridSystem().drawDebugGrid();
    }

    public void MoveSquad(SquadMovementHandler smh, int destinationX, int destintaionY)
    {
        
    }
}