using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PathFinder
{
    /*TODO:
     * Have different tiles of varying costs in the MOVE_STRAIGHT_CODE and MOVE_DIAGONAL_COST
     * MOVE_DIAGONAL_COST = sqrt(a^2 + b^2)
     */
    //Cost moving per square
    private int MOVE_STRAIGHT_COST = 10;

    //a*Sqrt(2) = 14.14, where a = 10
    private int MOVE_DIAGONAL_COST = 14;

    //Singleton instance of the path finder for moving units
    public static PathFinder instance { get; private set; }

    private GridSystem<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    Vector3 origin = Vector3.zero;

    public PathFinder(int width, int height) : this(width, height, new Vector3(-Mathf.Round(width * 2f) / 4f,  -Mathf.Round(width * 2f) / 4f)) {}

    public PathFinder(int width, int height, Vector3 origin)
    {
        instance = this;
        this.origin = origin;
        grid = new GridSystem<PathNode>(width, height, 1f, origin, (GridSystem<PathNode> g, int x, int y) => (new PathNode(g, x, y)));
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);

        if(path == null) return null;

        List<Vector3> vectorPath = new();

        foreach(PathNode node in path) vectorPath.Add((new Vector3(node.x, node.y) * grid.getCellSize() + Vector3.one * grid.getCellSize() * 0.5f) + origin);
        //for(int i = 0; i < vectorPath.Count; i++) vectorPath[i] += origin;
        return vectorPath;
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY, bool diagonal = true)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if(startNode == null || endNode == null || !startNode.isWalkable || !endNode.isWalkable) return null;

        openList = new(){ startNode };
        closedList = new();

        for(int x = 0; x < grid.getWidth(); x++)
        {
            for(int y = 0; y < grid.getHeight(); y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                node.gCost = int.MaxValue;
                node.CalcFCost();
                node.parentNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = calculateDistanceCost(startNode, endNode);
        startNode.CalcFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode) return CalculatePath(endNode);

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            //List<PathNode> neighborhood = getNeighboringNodes(currentNode);

            foreach(PathNode neighbor in getNeighboringNodes(currentNode, diagonal))
            {
                if(!neighbor.isWalkable) closedList.Add(neighbor); 
                if(closedList.Contains(neighbor)) continue;

                int tentativeGCost = currentNode.gCost + calculateDistanceCost(currentNode, neighbor);

                if(tentativeGCost < neighbor.gCost)
                {
                    neighbor.parentNode = currentNode;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = calculateDistanceCost(neighbor, endNode);
                    neighbor.CalcFCost();

                    if(!openList.Contains(neighbor)) openList.Add(neighbor);
                    
                }
            }
        }
        
        return null;
    }

    private List<PathNode> getNeighboringNodes(PathNode currentNode, bool diagonal = true)
    {
        List<PathNode> list = new();

        if(currentNode.x - 1 >= 0)
        {
            //Left
            list.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y));
            //Left down
            if(diagonal && currentNode.y - 1 >= 0) list.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y - 1));
            //Left up
            if(diagonal && currentNode.y + 1 < grid.getHeight()) list.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y + 1));
        }

        if(currentNode.x + 1 < grid.getWidth())
        {
            //Right
            list.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y));
            //Right down
            if(diagonal && currentNode.y - 1 >= 0) list.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y - 1));
            //Right up
            if(diagonal && currentNode.y + 1 < grid.getHeight()) list.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y + 1));     
        }

        //Down
        if(currentNode.y - 1 >= 0) list.Add(grid.GetGridObject(currentNode.x, currentNode.y - 1));
        //Up
        if(currentNode.y + 1 < grid.getHeight()) list.Add(grid.GetGridObject(currentNode.x, currentNode.y + 1));

        return list;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new();

        for(PathNode current = endNode; current != null; current = current.parentNode) path.Add(current);

        path.Reverse();

        return path;
    }

    private PathNode GetLowestFCostNode(List<PathNode> nodes)
    {
        PathNode returnNode = nodes[0];

        for(int i = 1; i < nodes.Count; i++)
        {
            if(nodes[i].fCost < returnNode.fCost) returnNode = nodes[i];
        }

        return returnNode;
    }

    private int calculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Math.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    public GridSystem<PathNode> GetGridSystem()
    {
        return grid;
    }

    public bool IsPositionWalkable(int x, int y)
    {
        return GetGridSystem().GetGridObject(x, y).isWalkable;
    }

    public bool HasPath(int startX, int startY, int endX, int endY, bool diagonal = true)
    {
        return FindPath(startX, startY, endX, endY, diagonal).Count > 0;
    }
}

public class PathNode
{
    private GridSystem<PathNode> grid;
    public int x, y;
    public int gCost, hCost, fCost;
    public PathNode parentNode;
    public bool isWalkable = true;

    public PathNode(GridSystem<PathNode> grid, int x, int y, bool isWalkable) : this(grid, x, y)
    {
        this.isWalkable = isWalkable;
    }

    public PathNode(GridSystem<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetIsWalkable(bool value)
    {
        isWalkable = value;
    }

    public bool GetIsWalkable()
    {
        return isWalkable;
    }

    public void CalcFCost()
    {
        fCost = gCost + hCost;
    }

    public void GetNodePosition(out int x, out int y, out Vector3 Position)
    {
        GetNodePosition(out x, out y);
        Position = grid.GetWorldPosition(x, y); //Convert position to a world coordinate
    }

    public void GetNodePosition(out int x, out int y)
    {
        x = this.x;
        y = this.y;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})", x, y, isWalkable ? "T" : "F");
    }
}