using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;




public class Player : MonoBehaviour
{


    [SerializeField] Sprite[] TeamCircles;
    [SerializeField] GameObject CanvasPlayerInfos;
    [SerializeField] private float motionSpeed;

    private CharacterAnimation characterAnim;
    private Dictionary<string, TextMeshProUGUI> LPIValues = new Dictionary<string, TextMeshProUGUI>();
    private GameObject LPI;
    public Tilemap map;
    public List<Vector2Int> pathTiles = new List<Vector2Int>();
    public bool Animate = false;

    public Vector3Int playerPosition;
    public int playerId;
    public int playerTeam = 0;


    private int S_PM = 6;
    private int S_PA = 6;
    private int S_PV = 500;

    public List<Spell> playerSpells = new List<Spell>();

    public Observable<int> PM = new Observable<int>();
    public Observable<int> PA = new Observable<int>();
    public Observable<int> PV = new Observable<int>();




    private void Awake()
    {
        characterAnim = GetComponentInChildren<CharacterAnimation>();
    }

    private void Start()
    {
        Transform sprite = transform.GetChild(0);
        sprite.transform.rotation = Quaternion.Euler(90, 45, 0);

        SetLightPlayerInfo();

    }

    private void Update()
    {
        if (pathTiles.Count > 0)
        {
            MovePlayerInWorld();
        }
    }

    public void Create(Tilemap map ,int id, int team, Vector3Int position, List<Spell> spells)
    {
        this.map = map;
        playerId = id;
        playerTeam = team;
        playerPosition = position;
        playerSpells = spells;
    }

    public void Move(Vector3Int target, ref Tilemap map)
    {
        if (target == Vector3Int.back)
            return;

        if (!hasPM(target))
            return;

        List<Vector2Int> path = Pathfinding.AStar(playerPosition, target, false, ref map, playerId);

        if (path.Count - 1 > PM.Value)
            return;

        if (pathTiles.Count < 1)
        {
            pathTiles = path;
            PM.Value = PM.Value - pathTiles.Count + 1;

            if (!(path.Count > 4))
                motionSpeed = 4.5f;


            MovePlayerInWorld();
        }
    }

    public bool hasPM(Vector3Int position)
    {
        if (DistanceBetweenPosition(playerPosition, position) > PM.Value)
        {
            return false;
        };

        return true;
    }

    public void EndTurn()
    {
        PM.Value = S_PM;
        PA.Value = S_PA;
    }

    public void PlaceAt(Vector3Int position)
    {
        transform.position = map.GetCellCenterWorld(position) + Vector3.up / 2;
        playerPosition = Vector3Int.FloorToInt(position);
    }

    public void SetTeamCircle()
    {
        if (playerTeam == 0)
        {
            GameObject Circle = new GameObject("TeamCircle");
            Circle.transform.position = new Vector3(-0.06f, -0.48f, 0);
            Circle.transform.rotation = Quaternion.Euler(90, 90, 0);
            Circle.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            Circle.transform.SetParent(transform, false);

            Circle.AddComponent<SpriteRenderer>();
            Circle.GetComponent<SpriteRenderer>().sprite = TeamCircles[0];

        }
        else
        {
            GameObject Circle = new GameObject("TeamCircle");
            Circle.transform.position = new Vector3(-0.06f, -0.48f, 0);
            Circle.transform.rotation = Quaternion.Euler(90, 90, 0);
            Circle.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            Circle.transform.SetParent(transform, false); 

            Circle.AddComponent<SpriteRenderer>();
            Circle.GetComponent<SpriteRenderer>().sprite = TeamCircles[1];
        }
    }

    private void MovePlayerInWorld()
    {
        if (transform.position == map.GetCellCenterWorld(new Vector3Int(pathTiles.First().x, pathTiles.First().y, 0)) + Vector3.up / 2)
        {
            if (pathTiles.Count > 1)
            {
               characterAnim.WalkTo(pathTiles[0], pathTiles[1]);
            }
            else
            {
                characterAnim.StopAnimation();
                motionSpeed = 7;
            }

            playerPosition = Vector3Int.FloorToInt(new Vector3(pathTiles.Last().x, pathTiles.Last().y, 0));
            pathTiles.RemoveAt(0);

        }
        if (pathTiles.Count > 0)
            transform.position = Vector3.MoveTowards(transform.position, map.GetCellCenterWorld(new Vector3Int(pathTiles.First().x, pathTiles.First().y, 0)) + Vector3.up / 2, motionSpeed * Time.deltaTime);
    }

    public void onSelected()
    {
        ReduceAlpha();
    }

    public void onDeselect()
    {
        ResetAlpha();
    }

    public void CastSpell(Vector3Int cell)
    {
        ChangeDirection(cell);
        characterAnim.CastSpell();
    }

    public void ChangeDirection(Vector3Int cell)
    {

        Vector2Int position = new Vector2Int(playerPosition.x, playerPosition.y);
        Vector2Int target = new Vector2Int(cell.x, cell.y);

        characterAnim.LookToDirection(position, target);
    }

    public void Hit()
    {
        characterAnim.TakeDamage();
    }

    public void ShowLPI(bool show)
    {
        if (LPI == null)
            return;

        LPI.SetActive(show);
    }

    private void ReduceAlpha()
    {
        GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.75f);
    }

    private void ResetAlpha()
    {
        GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    private void SetLightPlayerInfo()
    {
        LPI = Instantiate(CanvasPlayerInfos);
        LPI.transform.SetParent(transform, false);
        LPI.transform.SetSiblingIndex(1);
        LPI.transform.rotation = Quaternion.Euler(0, 45, 0);


        TextMeshProUGUI[] TMPInfos = LPI.GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < TMPInfos.Length; i++)
            LPIValues.Add(TMPInfos[i].name, TMPInfos[i]);


        PM.Value = S_PM;
        PA.Value = S_PA;
        PV.Value = S_PV;

        PA.OnChanged += SetLightInfoPA;
        PM.OnChanged += SetLightInfoPM;
        PV.OnChanged += SetLightInfoPV;


        SetAllLightInformations();
        ShowLPI(false);
    }

    private void SetAllLightInformations()
    {
        LPIValues["HPText"].SetText($"{PV.Value} - {S_PV}");
        LPIValues["PAText"].SetText($"{PA.Value}");
        LPIValues["PMText"].SetText($"{PM.Value}");
    }

    private void SetLightInfoPM(Observable<int> o, int ov, int nv)
    {
        LPIValues["PMText"].SetText($"{nv}");
    }

    private void SetLightInfoPA(Observable<int> o, int ov, int nv)
    {
        LPIValues["PAText"].SetText($"{nv}");
    }

    private void SetLightInfoPV(Observable<int> o, int ov, int nv)
    {
        if (nv < 0)
            nv = 0;

        LPIValues["HPText"].SetText($"{nv} - {S_PV}");
    }

    static int DistanceBetweenPosition(Vector3Int a, Vector3Int b)
    {
        int xd = a.x > b[0] ? a.x - b.x : b.x - a.x;
        int yd = a.y > b.y ? a.y - b.y : b.y - a.y;

        return xd + yd;
    }
}