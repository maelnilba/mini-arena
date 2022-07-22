using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{

    private Tilemap map;
    private GameObject[] childGo;
    [SerializeField] GameObject cellPrefab;
    private GridController WorldHandler;

    private void Awake()
    {
        WorldHandler = FindObjectOfType<GridController>();
    }

    private void Start() // have to be in start and not awake
    {
        Dictionary<int, List<Vector3Int>> CellPlacement = new Dictionary<int, List<Vector3Int>>();
        List<Vector3Int> CellPlacementA = new List<Vector3Int>();
        List<Vector3Int> CellPlacementB = new List<Vector3Int>();
        int CenterX = 0;
        int CenterY = 0;
        int CellsCount = 0;
        map = GetComponent<Tilemap>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tf = transform.GetChild(i);
            Fightcell c = tf.GetComponent<Fightcell>();
            if (c)
            {
                Vector3Int position = map.WorldToCell(tf.position);

                CellsCount++;
                CenterX += position.x;
                CenterY += position.y;

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.gameObject = cellPrefab;
                bool click = c.Clikable;
                bool ob = c.Obstacle;
                Debug.Log(c);
                if (c.PlacementA)
                    CellPlacementA.Add(position);
                if (c.PlacementB)
                    CellPlacementB.Add(position);
                tile.gameObject.GetComponent<CellBase>().position = position;
                tile.gameObject.GetComponent<CellBase>().isClikable = click;
                tile.gameObject.GetComponent<CellBase>().isObstacle = ob;

                if (click)
                    map.SetTile(position, tile);
         
               Destroy(tf.gameObject);
            }

        }
        CenterX = Mathf.FloorToInt(CenterX / CellsCount);
        CenterY = Mathf.FloorToInt(CenterY / CellsCount);
        Vector2Int cellCenter = new Vector2Int(CenterX, CenterY);

        CellPlacement.Add(0, CellPlacementA);
        CellPlacement.Add(1, CellPlacementB);
        WorldHandler.MapLoaded(CellPlacement, cellCenter);
    }

}
