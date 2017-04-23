using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public IslandController owner;

    public enum TileType
    {
        Ground,
        Air,
        Mountain,
        Food
    }

    public TileType Type;

    public float Mass;
    
    public List<Transform> Corners = new List<Transform>();

    private void OnDestroy()
    {
        foreach (var corner in Corners)
            if (corner != null)
                Destroy(corner.gameObject);
    }

}
