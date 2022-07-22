using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Location
{
    public int X;
    public int Y;
    public int F;
    public int G;
    public int H;
    public Location Parent;
}

public static class Pathfinding 
{

    public static List<Vector2Int> AStar(Vector3Int startTile, Vector3Int targetTile, bool isDiagonal, ref Tilemap map, int playerId = 0, List<Vector3Int> tileMapList = null)
    {
        List<Vector2Int> astarpath = new List<Vector2Int>();
        List<Location> openList = new List<Location>();
        List<Vector3Int> tileList;
        if (tileMapList == null)
            tileList = new List<Vector3Int>();
        else
            tileList = tileMapList;

        List<Location> closedList = new List<Location>();
        List<Location> adjacentSquares;
        int lowest;
        int g = 0;
        Location current = new Location();
        Location start = new Location { X = startTile.x, Y = startTile.y };
        Location target = new Location { X = targetTile.x, Y = targetTile.y };
        
        openList.Add(start);

        while (openList.Count > 0)
        {
            lowest = openList.Min(l => l.F);
            current = openList.First(l => l.F == lowest);
            closedList.Add(current);
            openList.Remove(current);
            if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                break;
            adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, map, isDiagonal, playerId, tileList);
            g++;

            foreach (var adjacentSquare in adjacentSquares)
            {
                if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) != null)
                    continue;

                if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) == null)
                {
                    adjacentSquare.G = g;
                    adjacentSquare.H = ComputeHScore(adjacentSquare.X,
                        adjacentSquare.Y, target.X, target.Y, g);
                    adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                    adjacentSquare.Parent = current;

                    openList.Insert(0, adjacentSquare);
                }
                else
                {
                    if (g + adjacentSquare.H < adjacentSquare.F)
                    {
                        adjacentSquare.G = g;
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        }
        while (current != null)
        {
      
            astarpath.Insert(0, new Vector2Int(current.X, current.Y));
            current = current.Parent;
           
        }

        return astarpath;
    }

    static bool CheckIfEntity(Location loc, ref Tilemap map, int playerId, List<Vector3Int> tileList)
    {
        Vector3Int locp = new Vector3Int(loc.X, loc.Y, 1);
        if (tileList.IndexOf(locp) != -1)
        {
            return false;
        }
        if (tileList.IndexOf(locp - Vector3Int.forward) != -1)
        {
            return true;
        }

        Vector3Int p = new Vector3Int(loc.X, loc.Y, 0);
        if (!map.HasTile(p))
        {
            tileList.Add(locp);
            return false;
        }
        if (map.HasTile(p))
        {
            GameObject go = map.GetInstantiatedObject(p);
            CellBase cb = go.GetComponent<CellBase>();
            if (cb.isObstacle || !cb.isClikable)
            {
                tileList.Add(locp);
                return false;
            }
            if (cb.playerId != 0)
                if (cb.playerId != playerId)
                {
                    tileList.Add(locp);
                    return false;
                }
        }
        tileList.Add(locp - Vector3Int.forward);
        return true;
    }

    static List<Location> GetWalkableAdjacentSquares(int x, int y, Tilemap map, bool isDiagonal, int playerId, List<Vector3Int> tileList)
    {
        if (isDiagonal)
        {
            List<Location> proposedLocations = new List<Location>()
            {
                new Location { X = x, Y = y - 1 },
                new Location { X = x, Y = y + 1 },
                new Location { X = x - 1, Y = y },
                new Location { X = x + 1, Y = y },
                new Location { X = x + 1, Y = y + 1 },
                new Location { X = x + 1, Y = y - 1 },
                new Location { X = x - 1, Y = y + 1},
                new Location { X = x - 1, Y = y - 1 },
            };
            return proposedLocations.Where(l => CheckIfEntity(l, ref map, playerId, tileList)).ToList();
        }
        else
        {
            List<Location> proposedLocations = new List<Location>()
            {
                new Location { X = x, Y = y - 1 },
                new Location { X = x, Y = y + 1 },
                new Location { X = x - 1, Y = y },
                new Location { X = x + 1, Y = y },
            };
            return proposedLocations.Where(l => CheckIfEntity(l, ref map, playerId, tileList)).ToList();

        }

    }


    static int ComputeHScore(int x, int y, int targetX, int targetY, int cost)
    {
        return cost * (Mathf.Abs(targetX - x) + Mathf.Abs(targetY - y));
        // add if diagnol
        // Used when movement is in 8 directions
        // h(n) = Cost * max(abs(n.x - goal.x), abs(n.y - goal.y))
    }


}
