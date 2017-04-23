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
        Food, 
        Meteor, 
        Magnet, 
    }

    public TileType Type;

    public Tile ChildTile;

    public float Mass;
    
    public List<Transform> Corners = new List<Transform>();

    private void OnDestroy()
    {
        foreach (var corner in Corners)
            if (corner != null)
                Destroy(corner.gameObject);
        if (ChildTile != null)
            Destroy(ChildTile);
    }

}
