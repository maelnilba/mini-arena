using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum HighlightType
{
    None = 0,
    PM = 1,
    PO = 2,
    POLos = 3,
    Target = 4,
    AvailablePM = 5,
    NotAvailablePM = 6,
    PlacementA = 7,
    PlacementB = 8,
    Damage = 10,
    Buff = 9,

}

public class Highlight : MonoBehaviour
{
    [SerializeField] GameObject HighLightPM;
    [SerializeField] GameObject HighLightPO;
    [SerializeField] GameObject HighLightPOLos;
    [SerializeField] GameObject HighLightTarget;
    [SerializeField] GameObject HighLightAvailableMove;
    [SerializeField] GameObject HighLightNotAvailableMove;

    private List<Vector3Int> tiles = new List<Vector3Int>();
    private Sprite White;
    private Sprite BlueLight;
    private Sprite Orange;
    private Sprite Green;
    private Sprite Blue;
    private Sprite Red;
    private Sprite Olive;
    public Tilemap map;




    private void Awake()
    {

        White = getTexture(Color.white);
        BlueLight = getTexture(new Color(0, 0, 1, 0.5f));
        Green = getTexture(Color.green);
        Blue = getTexture(Color.blue);
        Red = getTexture(Color.red);
        Orange = getTexture(new Color(255f/255f, 127f/255f, 0f/255f));
        Olive = getTexture(new Color(50f/255f, 205f/255f, 50f/255f));

    }

    public void ForceClearHighlight()
    {
        tiles.Clear();
        map.ClearAllTiles();
    }

    public void ClearHighlight()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            map.SetTile(tiles[i], null);
        }
        tiles.Clear();
    }

    public void HighlightCells(List<Vector2Int> Cells, int type, bool clear)
    {
        if (clear)
            ClearHighlight();

        HighlightType HType = (HighlightType)type;
        for (int i = 0; i < Cells.Count; i++)
        {
            HighlightCell(Cells[i], HType);
        }
    }

    public void HighlightCell(Vector2Int Cell, HighlightType type)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        //switch (type)
        //{
        //    case HighlightType.PM:
        //        tile.sprite = Orange;
        //        break;
        //    case HighlightType.PO:
        //        tile.sprite = Blue;
        //        break;
        //    case HighlightType.POLos:
        //        tile.sprite = BlueLight;
        //        break;
        //    case HighlightType.Target:
        //        tile.sprite = Red;
        //        break;
        //    case HighlightType.AvailablePM:
        //        tile.sprite = Green;
        //        break;
        //    case HighlightType.NotAvailablePM:
        //        tile.sprite = Olive;
        //        break;
        //    case HighlightType.PlacementA:
        //        tile.sprite = Blue;
        //        break;
        //    case HighlightType.PlacementB:
        //        tile.sprite = Red;
        //        break;
        //    case HighlightType.Damage:
        //        tile.sprite = Red;
        //        break;
        //    case HighlightType.Buff:
        //        tile.sprite = Olive;
        //        break;
        //    default:
        //        tile.sprite = White;
        //        break;
        //}

        switch (type)
        {
            case HighlightType.PM:
                tile.gameObject = HighLightPM;
                break;
            case HighlightType.PO:
                tile.gameObject = HighLightPO;
                break;
            case HighlightType.POLos:
                tile.gameObject = HighLightPOLos;
                break;
            case HighlightType.Target:
                tile.gameObject = HighLightTarget;
                break;
            case HighlightType.AvailablePM:
                tile.gameObject = HighLightAvailableMove;
                break;
            case HighlightType.NotAvailablePM:
                tile.gameObject = HighLightNotAvailableMove;
                break;
            default:
                tile.gameObject = HighLightPM;
                break;
        }

        Vector3Int position = new Vector3Int(Cell.x, Cell.y, 0);
        tiles.Add(position);
        map.SetTile(position, tile);
        map.RefreshTile(position);
    }

    private Sprite getTexture(Color color)
    {
        Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBAFloat, false);

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 70);
        return sprite;
    }

}