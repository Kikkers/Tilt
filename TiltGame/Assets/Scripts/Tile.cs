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

    public TileCorner NW { get; set; }
    public TileCorner SW { get; set; }
    public TileCorner SE { get; set; }
    public TileCorner NE { get; set; }

    private void OnDestroy()
    {
        if (NW != null) { Destroy(NW.gameObject); }
        if (SW != null) { Destroy(SW.gameObject); }
        if (SE != null) { Destroy(SE.gameObject); }
        if (NE != null) { Destroy(NE.gameObject); }
    }

}
