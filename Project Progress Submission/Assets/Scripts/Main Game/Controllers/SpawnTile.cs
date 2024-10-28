using UnityEngine;

public class SpawnTile : InteractableObject
{
    [Header("Setting Controller Parent")]
    [SerializeField] public int ControllerNum = 0;

    [Header("Tile Position")]
    [SerializeField] int x, y;

    public void SetXY(out int x, out int y)
    {
        PathFinder.instance.GetGridSystem().GetXY(transform.position, out x, out y);
        this.x = x;
        this.y = y;
    }

    public void GetXY(out int x, out int y)
    {
        x = this.x;
        y = this.y;
    }
}