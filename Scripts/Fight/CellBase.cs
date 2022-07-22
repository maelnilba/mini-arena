using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEntity
{
    public int entityTypeId;
    public bool hasplayerIn;
    public bool allowAllowTrough;
}



public class CellBase : MonoBehaviour
{
    private GridController WorldHandler;
    public Vector3Int position;
    public bool isClikable;
    public bool isObstacle;
    public int playerId;

    // Start is called before the first frame update
    void Start()
    {
        WorldHandler = FindObjectOfType<GridController>(); //GetComponentInParent<GridController>();
    }


    private void OnMouseOver()
    {
        if (isClikable)
            WorldHandler.Hover(position);

    }

    private void OnMouseExit()
    {
        StartCoroutine(CatchOutmap());
    }

    private IEnumerator CatchOutmap()
    {
        yield return new WaitForEndOfFrame();

        if (WorldHandler.HoverPosition == position)
            WorldHandler.Hover(Vector3Int.back);
    }
}
