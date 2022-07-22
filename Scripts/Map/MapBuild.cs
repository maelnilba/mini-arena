using UnityEngine;

public class MapBuild : MonoBehaviour
{
    [SerializeField] public GameObject[] MapsPrefab;


    public GameObject getMapByIndex(int index)
    {
        if (MapsPrefab[index] == null)
            index = 0;
        return MapsPrefab[index];
    }
}
