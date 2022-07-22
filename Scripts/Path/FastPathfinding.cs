using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

public static class FastPathfinding 
{

    public static List<Vector2Int> FindPath(Vector3Int start, Vector3Int target, List<Vector3Int> gridList, int range)
    {
        int2 startPosition = new int2(start.x, start.y);
        int2 endPosition = new int2(target.x, target.y);

        int2 gridSize = new int2(range*2+1, range*2+1);
        //NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);
        NativeHashMap<int, PathNode> pathNodeHashmap = new NativeHashMap<int, PathNode>(gridSize.x*gridSize.x, Allocator.Temp);
        for (int i = 0; i < gridList.Count; i++)
        {
            PathNode pathNode = new PathNode();
            pathNode.x = gridList[i].x;
            pathNode.y = gridList[i].y;
            pathNode.index = CalculateIndex(gridList[i].x, gridList[i].y, gridSize.x);
            pathNode.gCost = int.MaxValue;
            pathNode.hCost = CalculateDistanceCost(new int2(gridList[i].x, gridList[i].y), endPosition);
            pathNode.CalculateFCost();

            pathNode.isWalkable = gridList[i].z == 0;
            pathNode.cameFromNodeIndex = -1000;
            //Debug.Log($"{pathNode.index} {pathNode.x} {pathNode.y}");
            pathNodeHashmap.Add(pathNode.index, pathNode);
            //pathNodeArray[pathNode.index] = pathNode;
        }

        // iswalkable here

        NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Temp);
        neighbourOffsetArray[0] = new int2(-1, 0);
        neighbourOffsetArray[1] = new int2(+1, 0);
        neighbourOffsetArray[2] = new int2(0, +1);
        neighbourOffsetArray[3] = new int2(0, -1);

        int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

        PathNode startNode = pathNodeHashmap[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)]; //pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x, range)];
        startNode.gCost = 0;
        startNode.CalculateFCost();

        pathNodeHashmap.Remove(startNode.index);
        pathNodeHashmap.Add(startNode.index, startNode);  //pathNodeArray[startNode.index] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestCostNodeIndex(openList, pathNodeHashmap);
            PathNode currentNode = pathNodeHashmap[currentNodeIndex]; //pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                int2 neigbourOffset = neighbourOffsetArray[i];
                int2 neigbourPosition = new int2(currentNode.x + neigbourOffset.x, currentNode.y + neigbourOffset.y);


                if (!IsPositionInsideGrid(neigbourPosition, gridList))
                {
                    continue;
                }

                int neigbourNodeIndex = CalculateIndex(neigbourPosition.x, neigbourPosition.y, gridSize.x);

                if (closedList.Contains(neigbourNodeIndex))
                {
                    continue;
                }

                PathNode neigbourNode = pathNodeHashmap[neigbourNodeIndex]; //pathNodeArray[neigbourNodeIndex];

                if (!neigbourNode.isWalkable)
                {
                    continue;
                }

                int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neigbourPosition);
                if (tentativeGCost < neigbourNode.gCost)
                {
                    neigbourNode.cameFromNodeIndex = currentNodeIndex;
                    neigbourNode.gCost = tentativeGCost;
                    neigbourNode.CalculateFCost();

                    pathNodeHashmap.Remove(neigbourNodeIndex);
                    pathNodeHashmap.Add(neigbourNodeIndex, neigbourNode); //pathNodeArray[neigbourNodeIndex] = neigbourNode;

                    if (!openList.Contains(neigbourNode.index))
                    {
                        openList.Add(neigbourNode.index);
                    }

                }
            }
        }

        PathNode endNode = pathNodeHashmap[endNodeIndex]; //pathNodeArray[endNodeIndex];
            List<Vector2Int> result = new List<Vector2Int>();

            if (endNode.cameFromNodeIndex == -1000)
            {
            // dont find
            //Debug.Log("dont find");
            }
            else
            {
                // find
                NativeList<int2> path = CalculatePath(pathNodeHashmap, endNode);
                for (int i = 0; i < path.Length; i++)
                {
                    result.Insert(0, new Vector2Int(path[i].x, path[i].y));
                }
                path.Dispose();
            }

            pathNodeHashmap.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();

            return result;
        
    }

    static NativeList<int2> CalculatePath(NativeHashMap<int, PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.cameFromNodeIndex == -1000)
        {
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1000)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return path;
        }
    }

    static bool IsPositionInsideGrid(int2 gridPosition, List<Vector3Int> list)
    {

        if (list.Contains(new Vector3Int(gridPosition.x, gridPosition.y, 0)))
            return true;

        return false;
    }

    static int CalculateIndex(int x, int y, int gridWidth)
    {
        return  x + y * gridWidth;
    }

    static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        //int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
        //int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
        //int remaining = Mathf.Abs(xDistance - yDistance);

        //return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        return Mathf.Abs(bPosition.x - aPosition.x) + Mathf.Abs(bPosition.y - aPosition.y);
    }

    static int GetLowestCostNodeIndex(NativeList<int> openList, NativeHashMap<int, PathNode> pathNodeHashmap)
    {
        PathNode lowestCostPathNode = pathNodeHashmap[openList[0]];

        for (int i = 1; i < openList.Length; i++)
        {
            PathNode testPathNode = pathNodeHashmap[openList[i]];
            if (testPathNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testPathNode;
            }
        }
        return lowestCostPathNode.index;
    }

    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;

        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }
    }
}
