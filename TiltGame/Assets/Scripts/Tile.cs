using System.Collections;
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

    [SerializeField] private float _mass;
    public float Mass { get { return _mass + _impactExtraMass; } }

    private float _impactExtraMass = 0;

    public void DoImpact()
    {
        _impactExtraMass = 1000;
        StartCoroutine(MassRestore());
    }

    private IEnumerator MassRestore()
    {
        for (int i = 0; i < 50; ++i)
        {
            yield return new WaitForFixedUpdate();
            _impactExtraMass = _impactExtraMass * 0.9f;
        }
        _impactExtraMass = 0;
    }
    
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
