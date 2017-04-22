using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IslandController : MonoBehaviour
{
    public Transform AgentParent;
    public Transform Pivot;
    public Transform COMIndicator;
    public Vector3 COMPivotOffset;
    
    [Space]

    public float AgentMassMultiplier = 1;
    public float TileMassMultiplier = 1;
    public float TotalMassMultiplier = 1;

    [Space]

    public GameObject AirPrefab;
    public GameObject GroundPrefab;
    public GameObject MountainPrefab;
    public GameObject FoodPrefab;
    
    [Space]

    public Tile.TileType TilePainter;
    
    public int Width = 1, Height = 1;

    private Tile[,] allTiles;

    private void OnEnable()
    {
        RecalculateMass();
    }

    private void CollectTiles()
    {

    }

    private void RecalculateMass()
    {
        // tiles
        if (allTiles == null)
        {
            allTiles = new Tile[Width, Height];
            for (int x = 0; x < transform.childCount; ++x)
            {
                var column = transform.GetChild(x);
                for (int y = 0; y < transform.childCount; ++y)
                {
                    var tile = column.GetChild(y).GetComponent<Tile>();
                    allTiles[x, y] = tile;
                }
            }
        }

        // center of mass
        Vector3 centerOfMass = Vector3.zero;
        float massSum = 0;
        foreach(Tile t in allTiles)
        {
            var pos = t.transform.position;
            var mass = t.Mass * TileMassMultiplier;
            centerOfMass += pos * mass;
            massSum += mass;
        }
        var agents = AgentParent.GetComponentsInChildren<AgentController>();
        foreach (AgentController a in agents)
        {
            var pos = a.transform.position;
            var mass = a.Mass * AgentMassMultiplier;
            centerOfMass += pos * mass;
            massSum += mass;
        }
        if (massSum != 0)
            centerOfMass /= massSum;

        COMPivotOffset = (centerOfMass - Pivot.position) * TotalMassMultiplier;
        COMPivotOffset.y = 1;
        COMIndicator.position = Pivot.position + COMPivotOffset;
    }

    private void Awake()
    {
        Assert.IsNotNull(GroundPrefab);
        Assert.IsNotNull(MountainPrefab);
        Assert.IsNotNull(FoodPrefab);
        Assert.IsNotNull(AgentParent);
        Assert.IsNotNull(Pivot);
        Assert.IsNotNull(COMIndicator);
    }
    
    #region tiles

    private Tile BuildFood(int x, int y)
    {
        return Instantiate(FoodPrefab).GetComponent<Tile>();
    }

    private Tile BuildMountain(int x, int y)
    {
        return Instantiate(MountainPrefab).GetComponent<Tile>();
    }

    private Tile BuildGround(int x, int y)
    {
        return Instantiate(GroundPrefab).GetComponent<Tile>();
    }

    private Tile BuildAir(int x, int y)
    {
        return Instantiate(AirPrefab).GetComponent<Tile>();
    }

    private Tile CreateNewTile(int x, int y, Tile.TileType type)
    {
        switch (type)
        {
            case Tile.TileType.Food:
                return BuildFood(x, y);
            case Tile.TileType.Mountain:
                return BuildMountain(x, y);
            case Tile.TileType.Ground:
                return BuildGround(x, y);
            default:
                return BuildAir(x, y);
        }
    }
    
    public void ReplaceTile(Tile oldTile, Tile.TileType newType)
    {
        Tile parentTile;
        while ((parentTile = oldTile.transform.parent.GetComponent<Tile>()) != null)
            oldTile = parentTile;

        int x = (int)oldTile.transform.localPosition.x;
        int y = (int)oldTile.transform.localPosition.z;
        Tile newTile = CreateNewTile(x, y, newType);
        newTile.transform.SetParent(oldTile.transform.parent);
        newTile.transform.SetSiblingIndex(oldTile.transform.GetSiblingIndex());
        Destroy(oldTile.gameObject);
        var pos = newTile.transform.localPosition;
        pos.x = x;
        pos.z = y;
        newTile.transform.localPosition = pos;
        newTile.gameObject.name = "Tile " + y + ": " + newType.ToString();
        allTiles[x, y] = newTile;
    }

    #endregion

    void FixedUpdate()
    {
        #region Editor
        if (Application.isEditor)
        {
            if (GroundPrefab == null || MountainPrefab == null || FoodPrefab == null)
                return;

            // resizing
            Width = Width < 1 ? 1 : Width;
            Height = Height < 1 ? 1 : Height;

            bool dirty = false;
            while (transform.childCount < Width)
            {
                var go = new GameObject("Column " + transform.childCount);
                go.transform.SetParent(transform);
                dirty = true;
            }
            while (transform.childCount > Width)
            {
                Destroy(transform.GetChild(transform.childCount - 1));
                dirty = true;
            }
            if (dirty)
            {
                for (int x = 0; x < transform.childCount; ++x)
                {
                    Transform column = transform.GetChild(x);
                    while (column.childCount < Height)
                    {
                        int y = column.childCount;
                        Tile t = CreateNewTile(x, y, TilePainter);
                        t.transform.SetParent(column);
                        var pos = t.transform.localPosition;
                        pos.x = x;
                        pos.z = y;
                        t.transform.localPosition = pos;
                        t.gameObject.name = "Tile " + y + ": " + t.Type.ToString();
                    }
                    while (column.childCount > Height)
                    {
                        Destroy(column.GetChild(column.childCount - 1));
                    }
                }
                allTiles = null;
            }

            if (Input.GetKey(KeyCode.P) && Camera.current != null)
            {
                Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                if (hit.collider != null)
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null && tile.Type != TilePainter)
                        ReplaceTile(tile, TilePainter);
                }
            }
        }
        #endregion

        RecalculateMass();
    }
}
