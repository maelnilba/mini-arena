using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FightActionsDisplay : MonoBehaviour
{

    private bool isInitialized;
    private bool isPlacement;
    private bool isFight;

    private List<Vector3Int> CellPlacementA;
    private List<Vector3Int> CellPlacementB;
    private int selectedPlayerId = -1;

    public bool isLookingForRange = false;
    private bool isLookingForMove = true;
    private bool canGoToPosition = true;
    private List<Vector2Int> noLosSelectionRange = new List<Vector2Int>();
    private List<Vector2Int> noLosSelectionPM = new List<Vector2Int>();


    Dictionary<string, List<Vector2Int>> rangeSelectionPO = null;
    Dictionary<string, List<Vector2Int>> rangeSelectionPM = null;

    private Vector3Int cellHover;
    public bool isMyTurn = false;
    private Player currentPlayer;
    private int currentSpellIndex = -1;

    private List<int> myTeamPlayers = new List<int>();
    private Dictionary<string, Tilemap> tilemaps;
    private Tilemap map;
    private Highlight highlight;
    private GridController fightHandler;
    private FightUIManager fightUI;


    private void Awake()
    {
        tilemaps = new Dictionary<string, Tilemap>();
        foreach (Transform tilemap in this.transform)
        {
            tilemaps.Add(tilemap.name, tilemap.GetComponent<Tilemap>());
        }

        Transform MapTransform = transform.GetChild(0);
        foreach (Transform tilemap in MapTransform)
        {
            tilemaps.Add(tilemap.name, tilemap.GetComponent<Tilemap>());
        }

        map = tilemaps["Map"];
        highlight = GetComponent<Highlight>();
        highlight.map = tilemaps["Highlight"];
        fightHandler = GetComponentInParent<GridController>();
        fightUI = GetComponentInParent<FightUIManager>();
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        if (isPlacement)
            HandlePlacementDisplay();


        if (isFight)
            HandleFightDisplay();
    }

    public void SwitchToPlacement()
    {
        isInitialized = true;
        isPlacement = true;
    }

    public void SwitchToFight()
    {
        selectedPlayerId = -1;
        isPlacement = false;
        isFight = true;
    }

    public void SetPlacementCells(List<Vector3Int> CellsA, List<Vector3Int> CellsB)
    {
        CellPlacementA = CellsA;
        CellPlacementB = CellsB;
    }

    public void SetCurrentPlayer(Player player)
    {
        rangeSelectionPO = null;
        rangeSelectionPM = null;
        currentPlayer = player;
    }

    public void SetMyTeamPlayer(List<int> ps)
    {
        myTeamPlayers = ps;
    }

    public void SpellCast()
    {
        // blablabla check
        fightHandler.PlayerSpellCast(currentPlayer.playerId, cellHover);
    }

    public void SpellCastTimeline()
    {
        DoAction();
        ToggleLookingFor();
        if (isMyTurn)
            SpellCast();
    }

    public bool ConfirmationCanSpellCastFromTimeline()
    {
    
        if (!cellNoTargeable())
            if (isMyTurn)
                return true;

        return false;
    }

    public void SetCellHover(Vector3Int position)
    {
        cellHover = position;
    }

    public void SetSelectedSpell(int spellIndex)
    {
        currentSpellIndex = spellIndex;
        rangeSelectionPO = null;
        isLookingForMove = false;
        isLookingForRange = true;
    }

    public void DrawAvailablePM()
    {
        if (isLookingForRange)
            return;

        if (!isLookingForMove)
            return;
        if (isWalking())
            return;
        int currentPlayerPM = currentPlayer.PM.Value;
        if (currentPlayerPM < 1)
        {
            highlight.ClearHighlight();
            return;
        }

        Vector3Int currentPlayerPosition = currentPlayer.playerPosition;
     
        displayHighlightAvailablePMcell(currentPlayerPM, currentPlayerPosition);        
    }

    public void DrawAvailablePMFromPlayer(Player p)
    {
        if (isLookingForRange)
            return;
        if (p.playerId == currentPlayer.playerId)
            return;

        int PM = p.PM.Value;
        if (PM < 1)
            return;

        displayHighlightAvailablePMcellFromPlayer(PM, p.playerPosition, p.playerId);
    }

    public void ResetCurrentDisplay()
    {
        highlight.ClearHighlight();
        noLosSelectionPM = null;
        noLosSelectionRange = null;

        isLookingForMove = true;
        isLookingForRange = false;

        // selectedspell blabla
    }

    public void ClearHighlight()
    {
        highlight.ClearHighlight();
    }

    private void HandlePlacementDisplay()
    {
        if (CellPlacementA != null && CellPlacementB != null)
            DrawPlacementcell();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedPlayerId == -1)
            {
                if (map.HasTile(cellHover))
                {
                    GameObject go = map.GetInstantiatedObject(cellHover);
                    CellBase cb = go.GetComponent<CellBase>();
                    if (cb.isClikable && !cb.isObstacle && cb.playerId != 0 && isOnMyTeam(cb.playerId))
                    {
                        fightHandler.SelectPlayerById(cb.playerId);
                        selectedPlayerId = cb.playerId;
                    }
                }
            }
            else
            {
                if (isOutMap())
                {
                    fightHandler.CancelSelect(selectedPlayerId);
                    selectedPlayerId = -1;
                    return;
                }

                if (map.HasTile(cellHover))
                {
                    GameObject go = map.GetInstantiatedObject(cellHover);
                    CellBase cb = go.GetComponent<CellBase>();
                    if (cb.isClikable && !cb.isObstacle)
                    {
                        if (cb.playerId == selectedPlayerId)
                        {
                            fightHandler.CancelSelect(selectedPlayerId);
                            selectedPlayerId = -1;
                            return;
                        }

                        if (cb.playerId != 0 && cb.playerId != selectedPlayerId && fightHandler.CheckIfSameTeam(selectedPlayerId, cb.playerId))
                        {
                            fightHandler.SwapPlacement(selectedPlayerId, cb.playerId);
                            selectedPlayerId = -1;
                        }
                        else if (cb.playerId == 0 && fightHandler.CheckIfCellIsAbleToPlacement(cellHover, selectedPlayerId))
                        {
                            fightHandler.ChangePlacement(selectedPlayerId, cellHover);
                            selectedPlayerId = -1;
                        }
                    }
                    else
                    {
                        fightHandler.CancelSelect(selectedPlayerId);
                        selectedPlayerId = -1;
                    }
                } else
                {
                    fightHandler.CancelSelect(selectedPlayerId);
                    selectedPlayerId = -1;
                }
            }

        }
    }

    private bool MouseInput()
    {
        return Input.GetMouseButtonDown(0) && !fightUI.PointerOnUI;
    }

    private void HandleFightDisplay()
    {
        if (isLookingForMove && !isWalking() && isMyTurn)
        {
            DrawPath();

            if ( MouseInput() && canGoToPosition)
            {
                if (!isOutMap() && !onEntity() && onSelection())
                {
                    DoAction();
                    fightHandler.MoveCurrentPlayer(cellHover);

                }
            }

        }
        else if (isLookingForRange)
        {
            DrawSpellRange();
            if (MouseInput())
            {
                if ((isOutMap() || onEntity()) && currentSpellIndex != -1)
                {
                    ToggleLookingFor();
                }

                if (!cellNoTargeable())
                {
                    DoAction();
                    ToggleLookingFor();
                    if (isMyTurn)
                        SpellCast();
                } else
                {
                    ToggleLookingFor();
                    ClearHighlight();
                    DrawAvailablePM();
                }

            }
        }

    }

    private bool isOnMyTeam(int id)
    {
        if (myTeamPlayers.Contains(id))
            return true;
        return false;
    }

    private bool onSelection()
    {
        if (noLosSelectionPM.IndexOf(new Vector2Int(cellHover.x, cellHover.y)) != -1)
            return true;
        return false;
    }

    private bool isOutMap()
    {
        if (cellHover == Vector3Int.back)
            return true;
        return false;
    }

    private bool onEntity()
    {

        if (!map.HasTile(cellHover))
            return true;
        if (map.HasTile(cellHover))
        {
            GameObject go = map.GetInstantiatedObject(cellHover);

            if (go.GetComponent<CellBase>().isObstacle || !go.GetComponent<CellBase>().isClikable)
            {
                return true;
            }
        }
         
        return false;
    }

    private bool cellNoTargeable()
    {
        if (noLosSelectionRange.IndexOf(new Vector2Int(cellHover.x, cellHover.y)) == -1)
            return true;
        return false;
    }

    private bool isWalking()
    {
        List<Vector2Int> currentPlayerPath = currentPlayer.pathTiles;
        return currentPlayerPath.Count > 0 ? true : false;
    }

    private void DrawPlacementcell()
    {
        List<Vector2Int> cellsA = new List<Vector2Int>();
        List<Vector2Int> cellsB = new List<Vector2Int>();

        for (int i = 0; i < CellPlacementA.Count; i++)
        {
            cellsA.Add((Vector2Int)CellPlacementA[i]);
        }

        for (int i = 0; i < CellPlacementB.Count; i++)
        {
            cellsB.Add((Vector2Int)CellPlacementB[i]);
        }

        displayPlacementcell(0, cellsA);
        displayPlacementcell(1, cellsB);
    }

    private void displayPlacementcell(int team, List<Vector2Int> cells)
    {
        if (team == 0)
            highlight.HighlightCells(cells, 7, true);
        if (team == 1)
            highlight.HighlightCells(cells, 8, false);

    }

    private void DrawSpellRange()
    {
        if (currentSpellIndex == -1)
            return;

        Vector3Int currentPlayerPosition = currentPlayer.playerPosition;
        Spell selectedSpell = currentPlayer.playerSpells[currentSpellIndex]; 

        displayHighlightRangePOcell(selectedSpell, currentPlayerPosition);

        if (noLosSelectionRange.IndexOf(new Vector2Int(cellHover.x, cellHover.y)) == -1)
            return;
         if (isOutMap())
            return;

         foreach (SpellEffect effect in selectedSpell.effects)
        {
            List<Vector2Int> zoneEffect = Selection.SelectionEffectRange(effect, currentPlayerPosition, cellHover, ref map);

            if (zoneEffect.Count > 0)
                highlight.HighlightCells(zoneEffect, (int)SpellUtils.EffectTypeToHighlighType(effect.effectType), false);
        }
    }

    private void displayHighlightRangePOcell(Spell spell, Vector3Int position)
    {

        if (rangeSelectionPO == null)
            rangeSelectionPO = Selection.SelectionRange(spell, position, ref map);

        highlight.HighlightCells(rangeSelectionPO["noLosSelection"], 2, true);
        highlight.HighlightCells(rangeSelectionPO["losSelection"], 3, false);
        noLosSelectionRange = rangeSelectionPO["noLosSelection"];

    }

    private void displayHighlightAvailablePMcell(int PM, Vector3Int position)
    {

        if (rangeSelectionPM == null)
            rangeSelectionPM = Selection.SelectionAvailablePM(PM, position, ref map, currentPlayer.playerId);
       
        highlight.HighlightCells(rangeSelectionPM["noLosSelection"], 5, true);
        noLosSelectionPM = rangeSelectionPM["noLosSelection"];

    }

    private void displayHighlightAvailablePMcellFromPlayer(int PM, Vector3Int position, int id)
    {
        Dictionary<string, List<Vector2Int>> pmSelection = Selection.SelectionAvailablePM(PM, position, ref map, id);
        highlight.HighlightCells(pmSelection["noLosSelection"], 5, false);
    }

    private void DrawPath()
    {
        if ( isOutMap() || onEntity())
        {
            highlight.ClearHighlight();
            DrawAvailablePM();
            return;
        }
        displayHighlightPMcell();
    }

    private void displayHighlightPMcell()
    {
        Vector3Int currentPlayerPosition = currentPlayer.playerPosition;
        int currentPlayerPM = currentPlayer.PM.Value;

        List<Vector2Int> pathMovement;
        pathMovement = Pathfinding.AStar(currentPlayerPosition, cellHover, false, ref map, currentPlayer.playerId);
        int pathLen = pathMovement.Count - 1; 
        if (pathLen > currentPlayerPM)
        {
            canGoToPosition = false;
            return;
        }

        if (pathMovement.Count > 0)
        {
            pathMovement.RemoveAt(0);
        } else
        {
            canGoToPosition = false;
            return;
        }

        if (pathMovement.LastOrDefault() != new Vector2Int(cellHover.x, cellHover.y))
        {
            canGoToPosition = false;
            return;
        } else
        {
            canGoToPosition = true;
        }

        highlight.HighlightCells(pathMovement, 1, false);
    }

    private void ToggleLookingFor()
    {
        if (isLookingForMove && !isLookingForRange)
        {
            isLookingForMove = false;
            isLookingForRange = true;
        }
        else
        {
            currentSpellIndex = -1;
            rangeSelectionPO = null;
            isLookingForMove = true;
            isLookingForRange = false;
        }
    }

    private void DoAction()
    {
        rangeSelectionPO = null;
        rangeSelectionPM = null;
        highlight.ClearHighlight();
    }

    static int DistanceBetweenPosition(Vector3Int a, Vector3Int b)
    {
        int xd = a.x > b[0] ? a.x - b.x : b.x - a.x;
        int yd = a.y > b.y ? a.y - b.y : b.y - a.y;

        return xd + yd;
    }

    static int manhattanDistance(Vector2Int start, Vector2Int goal)
    {
        float xd = start.x - goal.x;
        float yd = start.y - goal.y;

        return Mathf.FloorToInt(Mathf.Abs(xd) + Mathf.Abs(yd));
    }
}