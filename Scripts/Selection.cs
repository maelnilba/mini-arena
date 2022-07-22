using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Point
{
    public Vector2Int p;
    public float dist;
}

public static class Selection
{

    public static Dictionary<string, List<Vector2Int>> SelectionAvailablePM(int PM, Vector3Int original, ref Tilemap map, int playerId)
    {
        Vector2Int origine = new Vector2Int(original.x, original.y);
        List<Vector2Int> allCells = getAllCells(PM, origine, ref map);

        List<Vector2Int> noLosSelection = new List<Vector2Int>();
        noLosSelection.Add(new Vector2Int(original.x, original.y));
        List<Vector2Int> losSelection = new List<Vector2Int>();

        List<Vector2Int> path;
        List<Point> orderedCell = new List<Point>();
        Vector2Int mp;
        Vector2Int cell;
        for (int i = 0; i < allCells.Count; i++)
        {
            mp = new Vector2Int(allCells[i].x, allCells[i].y);
            orderedCell.Add(new Point { p = mp, dist = Mathf.Abs(origine.x - mp.x) + Mathf.Abs(origine.y - mp.y) });
        }

        orderedCell = orderedCell.OrderByDescending(p => p.dist).ToList();
        List<Vector3Int> tileList = new List<Vector3Int>();

        for (int i = 0; i < orderedCell.Count; i++)
        {
            Vector3Int loc = new Vector3Int(orderedCell[i].p.x, orderedCell[i].p.y, 0);

            if (!map.HasTile(loc))
            {
                tileList.Add(loc + Vector3Int.forward);
                orderedCell.RemoveAt(i);
                i--;

            }
            else
            {
                GameObject go = map.GetInstantiatedObject(loc);
                CellBase cb = go.GetComponent<CellBase>();
                if (cb.isObstacle || (cb.playerId != 0 && cb.playerId != playerId) || !cb.isClikable)
                {
                    tileList.Add(loc + Vector3Int.forward);
                    orderedCell.RemoveAt(i);
                    i--;
                } else
                {
                    tileList.Add(loc);
                }
            }
        }

        // Fasted
        for (int i = 0; i < orderedCell.Count; i++)
        {
            cell = orderedCell[i].p;
            if (noLosSelection.IndexOf(cell) == -1)
            {
                path = FastPathfinding.FindPath(original, new Vector3Int(cell.x, cell.y, 0), tileList, PM);
                if (path.Count > 1)
                    path.RemoveAt(0);

                if (path.Count < PM + 1 && path.Count >= manhattanDistance(origine, cell) && path.Last() == cell)
                {
                    for (int k = 0; k < path.Count; k++)
                    {
                        if (noLosSelection.IndexOf(new Vector2Int(path[k].x, path[k].y)) == -1)
                        {
                            noLosSelection.Add(new Vector2Int(path[k].x, path[k].y));
                        }
                    }
                }
            }
        }

        for (int i = 0; i < allCells.Count; i++)
        {
            cell = allCells[i];
            if (noLosSelection.IndexOf(cell) == -1)
            {
                losSelection.Add(cell);
            }
        }

        for (int i = 0; i < noLosSelection.Count; i++)
        {
            if (noLosSelection[i].Equals(origine))
            {
                noLosSelection.RemoveAt(i);
                i--;
            }
        }

        Dictionary<string, List<Vector2Int>> selection = new Dictionary<string, List<Vector2Int>>();

        selection.Add("noLosSelection", noLosSelection);
        selection.Add("losSelection", losSelection);

        return selection;

    }

    public static Dictionary<string, List<Vector2Int>> SelectionRange(Spell spell, Vector3Int original, ref Tilemap map)
    {
        Vector2Int origine = new Vector2Int(original.x, original.y);

        List<Vector2Int> areaCells = new List<Vector2Int>();

        foreach (Area area in spell.areas)
        {
            switch (area)
            {
                case Area.None:
                    break;
                case Area.Normal:
                    areaCells.AddRange(GetNormalArea(spell.range, origine, ref map));
                    break;
                case Area.Diagonal:
                    areaCells.AddRange(GetDiagonalArea(spell.range, origine, ref map));
                    break;
                case Area.Line:
                    areaCells.AddRange(GetLineArea(spell.range, origine, ref map));
                    break;
                case Area.Square:
                    areaCells.AddRange(GetSquareArea(spell.range, origine, ref map));
                    break;
                default:
                    break;
            }
        }

        List<Vector2Int> allCells = areaCells.Distinct().ToList();


        List<Vector2Int> noLosSelection;
        List<Vector2Int> untargeableCells = new List<Vector2Int>();

        switch (spell.lineOfSigh)
        {
            case true:
                noLosSelection = LosDetector(allCells, origine, ref map);
                for (int i = 0; i < allCells.Count; i++)
                {
                    Vector2Int cell = allCells[i];
                    if (noLosSelection.IndexOf(cell) == -1)
                    {
                        untargeableCells.Add(cell);
                    }
                }

                for (int i = 0; i < noLosSelection.Count; i++)
                {
                    Vector3Int cellWorld = new Vector3Int(noLosSelection[i].x, noLosSelection[i].y, 0);
                    if (map.HasTile(cellWorld))
                    {
                        CellBase cell = map.GetInstantiatedObject(cellWorld).GetComponent<CellBase>();
                        if (cell.isObstacle || !cell.isClikable)
                        {
                            noLosSelection.RemoveAt(i);
                            i--;
                        }
                    }
                }

                for (int i = 0; i < untargeableCells.Count; i++)
                {
                    Vector3Int cellWorld = new Vector3Int(untargeableCells[i].x, untargeableCells[i].y, 0);
                    if (map.HasTile(cellWorld))
                    {
                        CellBase cell = map.GetInstantiatedObject(cellWorld).GetComponent<CellBase>();
                        if (cell.isObstacle || !cell.isClikable)
                        {
                            untargeableCells.RemoveAt(i);
                            i--;
                        }
                    }
                }
                break;
            case false:
                noLosSelection = allCells;
                break;
        }

        if (spell.minRange > 0)
        {
            List<Vector2Int> minareaCells = new List<Vector2Int>();
            foreach (Area area in spell.areas)
            {
                switch (area)
                {
                    case Area.None:
                        break;
                    case Area.Normal:
                        minareaCells.AddRange(GetNormalArea(spell.minRange - 1, origine, ref map));
                        break;
                    case Area.Diagonal:
                        minareaCells.AddRange(GetDiagonalArea(spell.minRange - 1, origine, ref map));
                        break;
                    case Area.Line:
                        minareaCells.AddRange(GetLineArea(spell.minRange - 1, origine, ref map));
                        break;
                    case Area.Square:
                        minareaCells.AddRange(GetSquareArea(spell.minRange - 1, origine, ref map));
                        break;
                    default:
                        break;
                }
            }
            noLosSelection.RemoveAll(cell => minareaCells.Contains(cell));
        }

        Dictionary<string, List<Vector2Int>> selection = new Dictionary<string, List<Vector2Int>>();

        selection.Add("noLosSelection", noLosSelection);
        selection.Add("losSelection", untargeableCells);

        return selection;

    }

    public static List<Vector2Int> SelectionEffectRange(SpellEffect effect, Vector3Int original, Vector3Int cible,ref Tilemap map)
    {
        Vector2Int origine = new Vector2Int(original.x, original.y);
        Vector2Int target = new Vector2Int(cible.x, cible.y);

        List<Vector2Int> selection = new List<Vector2Int>();

        if (!(effect.effectType == EffectType.Damage || effect.effectType == EffectType.Heal || effect.effectType == EffectType.Boost))
            return selection;

        switch (effect.area)
        {
            case Area.Mono:
                selection = GetMonoArea(0, target, ref map);
                break;
            case Area.Normal:
                selection = GetNormalArea(effect.areaRange, target, ref map);
                break;
            case Area.Diagonal:
                selection = GetDiagonalArea(effect.areaRange, target, ref map);
                break;
            case Area.HLine:
                selection = GetHLineArea(effect.areaRange, origine, target, ref map);
                break;
            case Area.VLine:
                selection = GetVLineArea(effect.areaRange, origine, target, ref map);
                break;
            case Area.Ring:
                selection = GetRingArea(effect.areaRange, target, ref map);
                break;
            case Area.Halfcircle:
                selection = GetHalfcircleArea(effect.areaRange, origine, target, ref map);
                break;
            case Area.Hammer:
                break;
            case Area.Cross:
                selection = GetLineArea(effect.areaRange, target, ref map);
                break;
        }

        return selection;
    }


    public static List<Vector2Int> getAllCells(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> allCells = new List<Vector2Int>();

        for (int i = -range; i <= range; i++)
        {
            for (int n = Mathf.Abs(i) - range; n <= Mathf.Abs(Mathf.Abs(i) - range); n++)
            {
                if (map.HasTile(new Vector3Int(i + origine.x, n + origine.y, 0)))
                {
                    allCells.Add(new Vector2Int(i + origine.x, n + origine.y));
                }
            }
        }

        return allCells;
    }

    static List<Vector2Int> LosDetector(List<Vector2Int> allCells, Vector2Int refPosition, ref Tilemap map )
    {
        List<Vector2Int> losSelection = getCells(allCells, refPosition, ref map);
        return losSelection;
    }

    static List<Vector2Int> GetNormalArea(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();

        for (int i = -range; i <= range; i++)
            for (int n = Mathf.Abs(i) - range; n <= Mathf.Abs(Mathf.Abs(i) - range); n++)
                if (map.HasTile(new Vector3Int(i + origine.x, n + origine.y, 0)))
                    area.Add(new Vector2Int(i + origine.x, n + origine.y));


        return area;
    }

    static List<Vector2Int> GetLineArea(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();


        for (int i = -range; i <= range; i++)
            if (map.HasTile(new Vector3Int(i + origine.x, origine.y, 0)))
                area.Add(new Vector2Int(i + origine.x, origine.y));


        for (int i = -range; i <= range; i++)
            if (map.HasTile(new Vector3Int(origine.x, i + origine.y, 0)))
                area.Add(new Vector2Int(origine.x, i + origine.y));
            
        

        return area;
    }

    static List<Vector2Int> GetDiagonalArea(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();

        for (int i = 0; i <= range; i++)
        {
            if (map.HasTile(new Vector3Int(origine.x + i, origine.y + i, 0)))
                area.Add(new Vector2Int(origine.x + i, origine.y + i));
            

            if (map.HasTile(new Vector3Int(origine.x - i, origine.y + i, 0)))
                area.Add(new Vector2Int(origine.x - i, origine.y + i));
            

            if (map.HasTile(new Vector3Int(origine.x + i, origine.y - i, 0)))
                area.Add(new Vector2Int(origine.x + i, origine.y - i));
            

            if (map.HasTile(new Vector3Int(origine.x - i, origine.y - i, 0)))
                area.Add(new Vector2Int(origine.x - i, origine.y - i));
        }

        return area;
    }

    static List<Vector2Int> GetSquareArea(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
                if (map.HasTile(new Vector3Int(origine.x + i, origine.y + j, 0)))
                    area.Add(new Vector2Int(origine.x + i, origine.y + j));

        return area;
    }

    static List<Vector2Int> GetMonoArea(int _, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        if (map.HasTile(new Vector3Int(origine.x, origine.y, 0)))
            area.Add(origine);
        return area;
    }

    static List<Vector2Int> GetVLineArea(int range, Vector2Int origine, Vector2Int target, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        Vector2Int direction = VectorDirection(origine, target);
        for (int i = 0; i < range; i++)
        {
            Vector3Int position = new Vector3Int((target + direction * i).x, (target + direction * i).y, 0);
            if (map.HasTile(position))
                area.Add((target + direction * i));
            
        }

        return area;
    }

    static List<Vector2Int> GetHLineArea(int range, Vector2Int origine, Vector2Int target, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        
        if (origine.x == target.x)
            for (int i = -range; i <= range; i++)
            {
                Vector3Int position = new Vector3Int(target.x + i,target.y,0);
                if (map.HasTile(position))
                    area.Add(new Vector2Int(target.x + i, target.y));

            }
        else if (origine.y == target.y)
            for (int i = -range; i <= range; i++)
            {
                Vector3Int position = new Vector3Int(origine.x, origine.y + i, 0);
                if (map.HasTile(position))
                    area.Add(new Vector2Int(target.x, target.y + i));

            }

        return area;
    }

    static List<Vector2Int> GetHalfcircleArea(int range, Vector2Int origine, Vector2Int target, ref Tilemap map)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        Vector2Int direction = VectorDirection(origine, target);

        if (origine.x == target.x)
            for (int i = -range; i <= range; i++)
            {
                Vector2Int position = new Vector2Int(target.x + i, target.y - Mathf.Abs(i) * direction.y);
              
                if (map.HasTile(new Vector3Int(position.x, position.y,0)))
                    area.Add(position);

            }
        else if (origine.y == target.y)
            for (int i = -range; i <= range; i++)
            {
                Vector2Int position = new Vector2Int(target.x - Mathf.Abs(i) * direction.x, target.y + i);
              
                if (map.HasTile(new Vector3Int(position.x,position.y, 0)))
                    area.Add(position);

            }

        return area;
    }

    static List<Vector2Int> GetRingArea(int range, Vector2Int origine, ref Tilemap map)
    {
        List<Vector2Int> area = GetNormalArea(range, origine, ref map);
        List<Vector2Int> outfill = GetNormalArea(range - 1, origine, ref map);
        area.RemoveAll(a => outfill.Contains(a));

        return area;
    }

    static List<Vector2Int> getCells(List<Vector2Int> allCells, Vector2Int refPosition, ref Tilemap map)
    {
        List<Vector2Int> range = allCells;
        List<Vector2Int> line = new List<Vector2Int>();
        bool los = false;
        string currentPoint = "";
        Vector2Int p = new Vector2Int();
        List<Point> orderedCell = new List<Point>();
        Vector2Int mp = new Vector2Int();

        for (int i = 0; i < range.Count; i++)
        {
            mp = new Vector2Int(range[i].x, range[i].y);
            orderedCell.Add(new Point { p = mp, dist = Mathf.Abs(refPosition.x - mp.x) + Mathf.Abs(refPosition.y - mp.y) });
        }

        orderedCell = orderedCell.OrderByDescending(p => p.dist).ToList();
       
        Dictionary<string, int> tested = new Dictionary<string, int>();
        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = 0; i < orderedCell.Count; i++)
        {
            p = orderedCell[i].p;
          
            if (!(tested.ContainsKey(string.Format("{0}_{1}", p.x, p.y))
                && refPosition.x + refPosition.y != p.x + p.y
                && refPosition.x - refPosition.y != p.x - p.y))
            {
                Vector2Int position = refPosition;
                line = getLine(position, p);
                

                if (line.Count == 0)
                {
                    result.Add(p);
                } else
                {
                    los = true;

                    for (int j = 0; j < line.Count; j++)
                    {
                        currentPoint = string.Format("{0}_{1}", line[j].x, line[j].y);
                        //Debug.Log(currentPoint);
              
                        if (j > 0 && hasEntity((int)Mathf.Floor(line[j - 1].x), (int)Mathf.Floor(line[j - 1].y), ref map))
                        {
                            los = false;
                            
                        } else if (
                            line[j].x + line[j].y == position.x + position.y ||
                            line[j].x - line[j].y == position.x - position.y)
                        {
                            // should be point los and checking on 
                            los = los && pointLos(line[j].x, line[j].y, true, ref map);
                            
                        } else if (!tested.ContainsKey(currentPoint))
                        {
                            los = los && pointLos(line[j].x, line[j].y, true, ref map);
                        } else
                        {
                            if (tested[currentPoint] == 1)
                            {
                                los = los && true;
                            } else
                            {
                                los = los && false;
                            }             
                        }
                    }
                    if (los)
                    {
                        tested[currentPoint] = 1;
                    }
                    else
                    {
                        tested[currentPoint] = 0;
                    }
                }
            }
        }

     
        for (int i = 0; i < range.Count; i++)
        {
            mp = range[i];
            if (tested.ContainsKey(string.Format("{0}_{1}",mp.x,mp.y)))
            {
                if (tested[string.Format("{0}_{1}", mp.x, mp.y)] == 1)
                {
                    result.Add(mp);
                }
            }
        }

        return result;
    }

    static List<Vector2Int> getLine(Vector2Int startCell, Vector2Int endCell)
    {
        List<Vector2Int> cellBetween = getCellsBetween(startCell, endCell);

        return cellBetween;
    }

    static List<Vector2Int> getCellsBetween(Vector2Int origine, Vector2Int target)
    {
        if (origine == target)
        {
            return new List<Vector2Int>();
        }
        Vector2Int originecoord = origine; // get x y
        Vector2Int targetcoord = target; // get x y
        if (originecoord == null || targetcoord == null)
        {
            return new List<Vector2Int>();
        }
        int dx = targetcoord.x - originecoord.x;
        int dy = targetcoord.y - originecoord.y;
        double _loc7_ = Math.Sqrt(dx * dx + dy * dy);
        double _loc8_ = dx / _loc7_;
        double _loc9_ = dy / _loc7_;
        double _loc10_ = Math.Abs(1 / _loc8_);
        double _loc11_ = Math.Abs(1 / _loc9_);
        int _loc12_ = _loc8_ < 0 ? -1 : 1;
        int _loc13_ = _loc9_ < 0 ? -1 : 1;
        double _loc14_ = 0.5 * _loc10_;
        double _loc15_ = 0.5 * _loc11_;
        List<Vector2Int> resultat = new List<Vector2Int>();
        while (originecoord.x != targetcoord.x || originecoord.y != targetcoord.y)
        {
            if (Math.Abs(_loc14_ - _loc15_) < 0.0001)
            {
                _loc14_ = _loc14_ + _loc10_;
                _loc15_ = _loc15_ + _loc11_;
                originecoord.x += _loc12_;
                originecoord.y += _loc13_;
            }
            else if (_loc14_ < _loc15_)
            {
                _loc14_ = _loc14_ + _loc10_;
                originecoord.x += _loc12_;
            }
            else
            {
                _loc15_ = _loc15_ + _loc11_;
                originecoord.y += _loc13_;
            }
            resultat.Add(new Vector2Int(originecoord.x,originecoord.y));
        }
        return resultat;
    }

    static bool hasEntity(int x, int y, ref Tilemap map)
    {
        Vector3Int loc = new Vector3Int(x, y, 0);
        if (!map.HasTile(loc))
            return false;
        if (map.HasTile(loc))
        {
            GameObject go = map.GetInstantiatedObject(loc);
            CellBase cb = go.GetComponent<CellBase>();
            if (cb.isObstacle || cb.playerId != 0)
            {
                return true;
            }
        }

        return false;
    }

    static bool pointLos(int x, int y, bool AllowTroughEntity, ref Tilemap map)
    {

        Vector3Int loc = new Vector3Int(x, y, 0);
        if (!map.HasTile(loc))
            return true;
        if (map.HasTile(loc))
        {
            GameObject go = map.GetInstantiatedObject(loc);
            if (!go.GetComponent<CellBase>().isClikable)
            {
                return true;
            }
            if (go.GetComponent<CellBase>().isObstacle)
            {
                return false;
            }
        }

        return true;
    }

    static int manhattanDistance(Vector2Int start, Vector2Int goal)
    {
        float xd = start.x - goal.x;
        float yd = start.y - goal.y;

        return Mathf.FloorToInt(Mathf.Abs(xd) + Mathf.Abs(yd));
    }

    static Vector2Int VectorDirection(Vector2Int origine, Vector2Int target)
    {
        Vector2Int HAUT = new Vector2Int(1, 0);
        Vector2Int DROITE = new Vector2Int(0, -1);
        Vector2Int BAS = new Vector2Int(-1, 0);
        Vector2Int GAUCHE = new Vector2Int(0, 1);
        Vector2Int CENTER = new Vector2Int(0, 0);

        if (target.x > origine.x && target.y == origine.y)
        {
            return HAUT;
        }
        else if (target.x < origine.x && target.y == origine.y)
        {
            return BAS;
        }
        else if (target.y < origine.y && target.x == origine.x)
        {
            return DROITE;
        }
        else if (target.y > origine.y && target.x == origine.x)
        {
            return GAUCHE;
        }
        else if (target == origine)
        {
            return CENTER;
        }

        return Vector2Int.zero;
    }
}