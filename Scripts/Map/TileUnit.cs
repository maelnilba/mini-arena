//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class TileUnit : MonoBehaviour
//{
//    public Tilemap tilemap;
//    public Tile tile;
//    public GridController Grid;
//    internal Vector3Int lastPosition;
//    [SerializeField] bool isClikable = true;
//    [SerializeField] bool isObstacle = false;
//    [SerializeField] GameObject fightCell;

//    bool isEnding;

//    private void Start()
//    {
//            if (tilemap == null)
//            {
//                tilemap = GetComponentInParent<Tilemap>();
//            }

//            if (tile == null)
//            {
//                tile = ScriptableObject.CreateInstance<Tile>();
//            }

//            lastPosition = tilemap.WorldToCell(transform.position);

//        if (tilemap.GetTile(lastPosition) == null)
//        {
//                tilemap.SetTile(lastPosition, tile);
//        }

             
//    }


//    public virtual void SetTile(Vector3Int position)
//    {
//        if (position != this.lastPosition)
//        {
//            ClearTile();

//            if (tilemap.GetTile(position) == null)
//            {
//                tilemap.SetTile(position, tile);
//            }

//            this.lastPosition = position;
//        }
//    }



//    public virtual void SetTile(Vector3 position)
//    {
//        SetTile(tilemap.WorldToCell(position));
//    }

//    public virtual void ClearTile()
//    {
//        if (tilemap.GetTile(lastPosition) == tile)
//        {
//            tilemap.SetTile(lastPosition, null);
//        }
//    }

//    void OnApplicationQuit()
//    {
//        isEnding = true;
//    }

//    void OnDestroy()
//    {
//        if (!isEnding)
//        {
//            ClearTile();
//        }
//    }
//}