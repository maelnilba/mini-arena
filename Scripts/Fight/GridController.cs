using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridController : MonoBehaviour
{

    [SerializeField] GameObject PlayerPrefab;

    public List<int> teamA = new List<int>(3) { 1 , 2, 3 };
    public List<int> teamB = new List<int>(3) { 4 , 5, 6 };

    private int myTeam = 0;

    public List<Player> timeline = new List<Player>();
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    public List<Vector3Int> CellPlacementA;
    public List<Vector3Int> CellPlacementB;

    private Vector2Int mapCenter;

    public Observable<int> currentPlayerTurn = new Observable<int>();
    public int turn = 0;
    private WaitForSeconds turnSeconds = new WaitForSeconds(60f);


    public Vector3Int HoverPosition;
    public Vector3Int LastHoverPosition;
    private bool isInitialized = false;
    private bool isPlacement = false;
    private bool isFight = false;
    private int pFocus = -1;


    public Dictionary<string, Tilemap> tilemaps;
    private Tilemap map;
    private FightActionsDisplay fightActionsDisplay;

    private FightUIManager fightUIController;


    void Awake()
    {
        fightUIController = GetComponent<FightUIManager>();
        currentPlayerTurn.Value = -1;

        SetupDisplay();
        SetupMap();
        SetupPlayers();

    }

    void Start()
    {
        SetupUIDisplay();
    }

    void Update()
    {
        if (!isInitialized)
            return;

        if (LastHoverPosition != HoverPosition)
        {
            LastHoverPosition = HoverPosition;
        }

        CheatActionSwitchTeam();

        if (isPlacement)
            HandlePlacement();

        if (isFight)
            HandleFight();

    }

    // PLACEMENT FUCTIONS
    public void SelectPlayerById(int id)
    {
            Player p = players[id];
            p.onSelected();
    }

    public void SwapPlacement(int id1, int id2)
    {
        CancelSelect(id1);

        if (!CheckIfSameTeam(id1, id2))
            return;

        Player p1 = players[id1];
        Player p2 = players[id2];

 
        GameObject go1 = map.GetInstantiatedObject(p1.playerPosition);
        go1.GetComponent<CellBase>().playerId = id2;
        map.RefreshTile(p1.playerPosition);

        GameObject go2 = map.GetInstantiatedObject(p2.playerPosition);
        go2.GetComponent<CellBase>().playerId = id1;
        map.RefreshTile(p2.playerPosition);

        Vector3Int savePosition = p1.playerPosition;
        p1.PlaceAt(p2.playerPosition);
        p2.PlaceAt(savePosition);

    }

    public bool CheckIfCellIsAbleToPlacement(Vector3Int cell, int id)
    {
        Player p = players[id];
        if (p.playerTeam == 0)
        {
            if (!CellPlacementA.Contains(cell))
                return false;

        }
        if (p.playerTeam == 1)
        {
            if (!CellPlacementB.Contains(cell))
                return false;

        }

        return true;
    }

    public bool CheckIfSameTeam(int id1, int id2)
    {
        Player p1 = players[id1];
        Player p2 = players[id2];

        if (p1.playerId == 0 || p1.playerId == 0)
            return false;

        if (p1.playerTeam == p2.playerTeam)
            return true;

        return false;
    }

    public void ChangePlacement(int id, Vector3Int cell)
    {
        CancelSelect(id);
        Player p = players[id];
        if (p.playerPosition == cell)
            return;

        if (!CheckIfCellIsAbleToPlacement(cell, id))
            return;

        Vector3Int lastPosition = p.playerPosition;
        GameObject go1 = map.GetInstantiatedObject(lastPosition);
        go1.GetComponent<CellBase>().playerId = 0;
        map.RefreshTile(lastPosition);

        GameObject go2 = map.GetInstantiatedObject(cell);
        go2.GetComponent<CellBase>().playerId = id;
        map.RefreshTile(cell);

        p.PlaceAt(cell);

    }

    public void CancelSelect(int id)
    {
        Player p = players[id];
        p.onDeselect();
    }

    //


    public void Hover(Vector3Int position)
    {
        if (!isInitialized)
            return;

        if (position != HoverPosition || position == Vector3Int.back) // Trigger only 1 and not every frame
        {
            if (isFight)
            {

                if (isMyTeamTurn())
                    fightActionsDisplay.DrawAvailablePM();
                else
                    fightActionsDisplay.ClearHighlight();
                HandlePlayerFocus(position);
            }

            //Debug.Log($"position {position}");
            LastHoverPosition = HoverPosition;
            HoverPosition = position;
            fightActionsDisplay.SetCellHover(position);
  
        }
    }

    public void NextPlayerTurn()
    {

        if (!isFight)
            return;

        TurnTimer();

        fightActionsDisplay.ResetCurrentDisplay();
        timeline[currentPlayerTurn.Value].EndTurn();

        if (currentPlayerTurn.Value == timeline.Count - 1)
        {
            currentPlayerTurn.Value = 0;
            turn++;
        }
        else
        {
            currentPlayerTurn.Value++;
        }

        if (isMyTeamTurn())
        {
            fightActionsDisplay.SetCurrentPlayer(timeline[currentPlayerTurn.Value]);
            fightActionsDisplay.isMyTurn = true;
            fightActionsDisplay.DrawAvailablePM();
        }
        else
        {
            fightActionsDisplay.SetCurrentPlayer(timeline[(currentPlayerTurn.Value + 1) % timeline.Count]);
            fightActionsDisplay.isMyTurn = false;
        }

    }

    public void MoveCurrentPlayer(Vector3Int position)
    {
        Vector3Int lastPosition = timeline[currentPlayerTurn.Value].playerPosition;
        if (position == lastPosition)
            return;
        GameObject go1 = map.GetInstantiatedObject(lastPosition);
        go1.GetComponent<CellBase>().playerId = 0;
        map.RefreshTile(lastPosition);

        GameObject go2 = map.GetInstantiatedObject(position);
        if (go2.GetComponent<CellBase>().playerId != 0)
            return;

        go2.GetComponent<CellBase>().playerId = timeline[currentPlayerTurn.Value].playerId;

        map.RefreshTile(position);

        timeline[currentPlayerTurn.Value].Move(position, ref map);

        if (isMyTeamTurn())
            StartCoroutine("DrawAvailablePMAfterRunning");
        
    }

    public void PlayerSpellCast(int id, Vector3Int position)
    {
        players[id].CastSpell(position);

        GameObject go = map.GetInstantiatedObject(position);
        int id2 = go.GetComponent<CellBase>().playerId;

        if (id2 != 0)
        {
            players[id2].Hit();
            players[id2].PV.Value -= 10;
        }

        if (isMyTeamTurn())
            fightActionsDisplay.DrawAvailablePM();
    }

    public void MapLoaded(Dictionary<int, List<Vector3Int>> cellPlacement, Vector2Int cellCenter)
    {
        mapCenter = cellCenter;
        CellPlacementA = cellPlacement[0];
        CellPlacementB = cellPlacement[1];
        for (int i = 0; i < timeline.Count; i++)
        {
            Player p = timeline[i];
            if (p.playerTeam == 0)
                p.PlaceAt(CellPlacementA[i % 3]);
            if (p.playerTeam == 1)
                p.PlaceAt(CellPlacementB[i % 3]);

            GameObject go = map.GetInstantiatedObject(p.playerPosition);
            go.GetComponent<CellBase>().playerId = p.playerId ;
            map.RefreshTile(p.playerPosition);
        }

        SwitchToPlacement();
    }

    private void StartFight()
    {
        
        if (currentPlayerTurn.Value == timeline.Count - 1)
        {
            currentPlayerTurn = 0;
            turn++;
        }
        else
        {
            currentPlayerTurn.Value++;
        }

        TurnTimer();

        if (isMyTeamTurn())
        {
            fightActionsDisplay.SetCurrentPlayer(timeline[currentPlayerTurn.Value]);
            fightActionsDisplay.isMyTurn = true;
            fightActionsDisplay.DrawAvailablePM();
        }
        else
        {
            fightActionsDisplay.SetCurrentPlayer(timeline[(currentPlayerTurn.Value + 1) % timeline.Count]);
            fightActionsDisplay.isMyTurn = false;
        }

    }

    private void HandlePlayerFocus(Vector3Int position)
    {
        if (position == HoverPosition)
            return;

        int e = isPlayerFocus(position);
        if (e != -1)
        {
            if (pFocus != -1 && isPlayerFocus(HoverPosition) == pFocus)
            {
                players[pFocus].ShowLPI(false);
            }

            pFocus = e;
            players[e].ShowLPI(true);
            if (isMyTeamTurn() && e != timeline[currentPlayerTurn.Value].playerId)
                fightActionsDisplay.DrawAvailablePMFromPlayer(players[e]);
            else
                fightActionsDisplay.DrawAvailablePMFromPlayer(players[e]);
            
        }
        else
        {
            if (pFocus != -1)
                players[pFocus].ShowLPI(false);
        }
    }

    private int isPlayerFocus(Vector3Int position)
    {
        foreach (Player p in players.Values)
        {
            if (p.playerPosition == position)
                return p.playerId;
        }

        return -1;
    }

    // HANDLERS
    private void HandleFight()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            fightActionsDisplay.SetSelectedSpell(0); 
        }
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            fightActionsDisplay.SetSelectedSpell(1); 
        }
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            fightActionsDisplay.SetSelectedSpell(2); 
        }

        if (Input.GetKeyDown(KeyCode.Space) && isMyTeamTurn())
        {
            // simulte pass turn
            NextPlayerTurn();
        }
    }

    private void HandlePlacement()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ReadyToFight();
        }
    }

    private void ReadyToFight(int team = 0) // would wait two team ready and would be on server side ofc
    {
        SwitchToFight();
    }

    private void SetupDisplay()
    {
        HoverPosition = Vector3Int.back;
        LastHoverPosition = Vector3Int.back;
        fightActionsDisplay = GetComponent<FightActionsDisplay>();
    }

    private void SetupMap()
    {
        MapBuild mb = GetComponent<MapBuild>();
        GameObject mapprefab = mb.getMapByIndex(2);
        GameObject mapgo = Instantiate(mapprefab);
        mapgo.transform.SetParent(gameObject.transform);
        mapgo.transform.SetSiblingIndex(0);
        tilemaps = new Dictionary<string, Tilemap>();
        Transform MapTransform = transform.GetChild(0);
        foreach (Transform tilemap in MapTransform)
        {
            tilemaps.Add(tilemap.name, tilemap.GetComponent<Tilemap>());
        }
        map = tilemaps["Map"];
    }

    private void SetupPlayers()
    {
        int iid = 0;

        for (int i = 0; i < teamA.Count; i++)
        {
            GameObject pgo = Instantiate(PlayerPrefab);
            pgo.transform.SetParent(map.transform);
            pgo.transform.SetSiblingIndex(iid);
            Vector3 plposition = new Vector3(iid + 0.5f, 0.5f, 0.5f);
            pgo.transform.position = plposition;
            Player pgoplayer = pgo.GetComponent<Player>();
            pgoplayer.Create(map, teamA[i], 0, map.WorldToCell(plposition - Vector3.up / 2), CreateDummySpells());
            players.Add(pgoplayer.playerId, pgoplayer);
            iid++;
        }

        for (int i = 0; i < teamB.Count; i++)
        {
            GameObject pgo = Instantiate(PlayerPrefab);
            pgo.transform.SetParent(map.transform);
            pgo.transform.SetSiblingIndex(iid);
            Vector3 plposition = new Vector3(iid + 0.5f, 0.5f, 0.5f);
            pgo.transform.position = plposition;
            Player pgoplayer = pgo.GetComponent<Player>();
            pgoplayer.Create(map, teamB[i], 1, map.WorldToCell(plposition - Vector3.up / 2), CreateDummySpells());
            players.Add(pgoplayer.playerId, pgoplayer);
            iid++;
        }

        for (int i = 0; i < teamA.Count + teamB.Count; i++)
        {
            if (i % 2 == 0)
            {
                timeline.Add(players[teamA[i % 3]]);
            } else
            {
                timeline.Add(players[teamB[i % 3]]);
            }
        }

     
        List<int> ps = new List<int>();

        for (int i = 0; i < timeline.Count; i++)
        {
            if (timeline[i].playerTeam == myTeam)
                ps.Add(timeline[i].playerId);
        }

        fightActionsDisplay.SetMyTeamPlayer(ps);
    }

    private void SetupUIDisplay()
    {
        fightUIController.Setup(timeline, currentPlayerTurn);
    }

    private void SwitchToPlacement()
    {
        isInitialized = true;
        isPlacement = true;
        fightUIController.Display(false);
        fightActionsDisplay.SwitchToPlacement();
        fightActionsDisplay.SetPlacementCells(CellPlacementA, CellPlacementB);
        PlayersLookAtCenter();
    }

    private void SwitchToFight()
    {
        isPlacement = false;
        isFight = true;
        fightActionsDisplay.SwitchToFight();
        fightUIController.Display(true);
        StartCoroutine("SetPlayerTeamCircles");
        StartFight();
    }

    private IEnumerator SetPlayerTeamCircles()
    {
        yield return new WaitForEndOfFrame();
        foreach (Player p in players.Values)
            p.SetTeamCircle();
    }

    private void PlayersLookAtCenter()
    {
        Vector3Int direction = new Vector3Int(mapCenter.x, mapCenter.y, 0);
        foreach (Player p in players.Values)
            p.ChangeDirection(direction);
        
    }

    private bool isMyTeamTurn()
    {
        if (currentPlayerTurn.Value == -1)
            return timeline[currentPlayerTurn.Value + 1].playerTeam == myTeam;
        return timeline[currentPlayerTurn.Value].playerTeam == myTeam;
    }

    private void TurnTimer()
    {
        StopCoroutine("SixtySeconds");

        StartCoroutine("SixtySeconds");
    }

    private IEnumerator SixtySeconds()
    {
        yield return turnSeconds;

        NextPlayerTurn();

    }

    private IEnumerator DrawAvailablePMAfterRunning()
    {
        while(timeline[currentPlayerTurn.Value].pathTiles.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        fightActionsDisplay.DrawAvailablePM();
    }

    private void CheatActionSwitchTeam()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isPlacement)
            {
                myTeam = myTeam == 0 ? 1 : 0;

                List<int> ps = new List<int>();

                for (int i = 0; i < timeline.Count; i++)
                {
                    if (timeline[i].playerTeam == myTeam)
                        ps.Add(timeline[i].playerId);
                }

                fightActionsDisplay.SetMyTeamPlayer(ps);

            }
            if (isFight)
            {
                myTeam = myTeam == 0 ? 1 : 0;
                
                fightActionsDisplay.isMyTurn = isMyTeamTurn();
                
                fightActionsDisplay.SetCurrentPlayer(timeline[currentPlayerTurn.Value]);
                fightActionsDisplay.DrawAvailablePM();
            }
        }

    }

    private List<Spell> CreateDummySpells()
    {
        List<Spell> dummyListSpell = new List<Spell>();

        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 0, 0, (int)Area.Normal, true, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Boost, BuffType.PM, 2, 2, (int)Area.Normal, 2, true)));
        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 4, 7, (int)Area.Normal, false, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Damage, BuffType.None, 10, 20, (int)Area.Normal, 2, true)));
        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 1, 4, (int)Area.Diagonal, true, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Damage, BuffType.None, 10, 20, (int)Area.Mono, 1, true)));
        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 1, 7, (int)Area.Line, true, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Damage, BuffType.None, 10, 20, (int)Area.Halfcircle, 3, true)));
        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 0, 7, (int)Area.Line, true, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Damage, BuffType.None, 10, 20, (int)Area.Cross, 3, true)));
        dummyListSpell.Add(new Spell("dummy", 0, 0, 3, 1, 2, (int)Area.Square, true, false, false, true, 0, 3, 0, 0, 0, new SpellEffect(0, EffectType.Damage, BuffType.None, 10, 20, (int)Area.Ring, 4, true)));


        return dummyListSpell;
    }
}
