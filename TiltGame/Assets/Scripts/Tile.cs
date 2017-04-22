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

}
