using Unity.VisualScripting;
using UnityEngine;

public class TileInformation
{
    int straightCost = 10;
    int diagonalCost = 10;

    public TileInformation()
    {
        straightCost = 10;
        diagonalCost = (int) (straightCost * Mathf.Sqrt(2));
    }

    public TileInformation(GridSystem<TileInformation> g, int x, int y)
    {
        
    }
}