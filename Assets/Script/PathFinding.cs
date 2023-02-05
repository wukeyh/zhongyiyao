using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;  //直线成本
    private const int MOVE_DIAGONAL_COST = 14; //对角线成本

    public static PathFinding Instance { get; private set;}
    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> clostList;

    public PathFinding(int width, int height)
    {
        Instance = this;
        grid = new Grid<PathNode>(width, height, 10f, Vector3.zero, (Grid<PathNode> grid, int x, int y) => new PathNode(grid, x, y));
    }

    public Grid<PathNode> GetGrid()
    {
        return grid;
    }

    //接收起点和终点，返回路径
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXZ(startWorldPosition, out int startX, out int startY);
        grid.GetXZ(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if(path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach(PathNode pathNode in path)
            {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        openList = new List<PathNode> {startNode};
        clostList = new List<PathNode>();

        //初始化节点
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode)              
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            clostList.Add(currentNode);

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if(clostList.Contains(neighbourNode))   continue;
                if(!neighbourNode.isWalkable)       //判断是否可行走
                {
                    clostList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if(tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if(!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        return null;
    }


    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if(currentNode.x - 1 >= 0)  //判断左边
        {
            //左边节点
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            //左下节点
            if(currentNode.y - 1 >= 0)  neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            //左上节点
            if(currentNode.y + 1 < grid.GetHeight())  neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if(currentNode.x + 1 >= 0)
        {
            //右边节点
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            //右下节点
            if(currentNode.y - 1 >= 0)  neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            //右上节点
            if(currentNode.y + 1 < grid.GetHeight())  neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        //正下方节点
        if(currentNode.y - 1 >= 0)  neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        //正上方节点
        if(currentNode.y + 1 < grid.GetHeight())  neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    //查找路径
    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remianing = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remianing;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)  //获得最低成本节点
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
