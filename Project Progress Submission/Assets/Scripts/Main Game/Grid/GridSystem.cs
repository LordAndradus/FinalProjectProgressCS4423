using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class GridSystem<T>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public T value;
        public int x;
        public int y;
    }

    int width;
    int height;
    float cellSize = 1f;
    Vector3 origin;

    T[,] grid;

    TextMesh[,] debugTextArray;

    GameObject ParentDebug;

    public GridSystem(int width, int height, float cellSize, Vector3 origin, Func<GridSystem<T>, int, int, T> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        grid = new T[width, height];

        for(int x = 0; x < grid.GetLength(0); x++)
        {
            for(int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public void MeshOutline(int x, int y)
    {
        //float thickness = 0.01f;
        Color outline = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        Vector3[] vertices = new Vector3[]{
            GetWorldPosition(x, y),
        };
    }

    public void DrawOutline(TextMesh tile, int x, int y)
    {
        float thickness = 0.01f;

        LineRenderer border = tile.AddComponent<LineRenderer>();
        border.positionCount = 5;
        border.loop = true;
        border.startWidth = thickness;
        border.endWidth = thickness;
        border.material = new Material(Shader.Find("Sprites/Default"));
        border.startColor = Color.yellow;
        border.endColor = Color.yellow;

        Vector3[] vertexes = new Vector3[border.positionCount];
        vertexes[0] = GetWorldPosition(x, y);
        vertexes[1] = GetWorldPosition(x, y + 1);
        vertexes[2] = GetWorldPosition(x + 1, y + 1);
        vertexes[3] = GetWorldPosition(x + 1, y);
        vertexes[4] = vertexes[0];

        border.SetPositions(vertexes);
        border.sortingOrder = 2;
        border.sortingLayerName = "GridOutline";
        
    }

    public void setDebugParent(Transform parent)
    {
        ParentDebug = parent.gameObject;
    }

    public void SetDebugText()
    {
        foreach(TextMesh t in debugTextArray)
        {
            GetXY(t.transform.position, out int x, out int y);
            t.text = "Tile_" + GetGridObject(x, y).ToString();
            t.name = "Tile_" + GetGridObject(x, y).ToString();
        }
    }

    public void drawDebugGrid()
    {
        if(ParentDebug == null)
        {
            Debug.LogError("No parent debug was specified!");
            return;
        }

        if(debugTextArray != null)
        {
            foreach(Transform child in ParentDebug.transform)
            {
                UnityEngine.Object.Destroy(child);
            }
        }

        debugTextArray = new TextMesh[width, height];
        
        for(int x = 0; x < grid.GetLength(0); x++)
        {
            for(int y = 0; y < grid.GetLength(1); y++)
            {
                debugTextArray[x, y] = UtilityClass.CreateWorldText(string.Format("Tile_(x={0},y={1})", x, y), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 100, Color.white, TextAnchor.MiddleCenter);
                debugTextArray[x, y].transform.localScale = new Vector3(cellSize * 0.01f, cellSize * 0.01f, cellSize * 0.01f);
                DrawOutline(debugTextArray[x, y], x, y);
                debugTextArray[x, y].transform.parent = ParentDebug.transform;
            }
        }

        SetDebugText();
    }

    public int getWidth()
    {
        return width;
    }

    public int getHeight()
    {
        return height;
    }

    public float getCellSize()
    {
        return cellSize;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - origin).x/ cellSize);
        y = Mathf.FloorToInt((worldPosition - origin).y/ cellSize);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + origin;
    }

    public void SetGridObject(int x, int y, T value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            grid[x, y] = value;
            if(debugTextArray != null) debugTextArray[x, y].text = grid[x, y].ToString();
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y, value = value });
        }
    }

    public T GetGridObject(Vector3 WorldPosition)
    {
        GetXY(WorldPosition, out int x, out int y);
        return GetGridObject(x, y);
    }

    public T GetGridObject(int x, int y)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            return grid[x, y];
        }

        Debug.Log("Went out of bounds in grid! (" + x + ", " + y + ")");
        throw new System.Exception(string.Format("Went of bounds ({0}, {1}), passed = ({2}, {3})", width, height, x, y));
    }
}
