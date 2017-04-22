using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IslandController : MonoBehaviour
{
    public Transform AgentParent;
    public Transform AgentSpawns;
    public Transform Pivot;
    public Transform COMIndicator;
    public Vector3 COMPivotOffset;

    public Tile HighlightedTile;
    
    [Space]

    public float AgentMassMultiplier = 1;
    public float TileMassMultiplier = 1;
    public float TotalMassMultiplier = 1;

    [Space]

    public GameObject AgentPrefab;
    public GameObject AirPrefab;
    public GameObject GroundPrefab;
    public GameObject MountainPrefab;
    public GameObject FoodPrefab;
    
    [Space]

    public Tile.TileType TilePainter;
    
    public int Width = 1, Height = 1;

    private RaycastHit _mouseHit = default(RaycastHit);
    private Tile[,] _allTiles;
    private bool _isSelecting;
    private Vector3 _selectStart;
    private List<AgentController> _agents = new List<AgentController>();
    private List<AgentController> _selectedAgents = new List<AgentController>();

    private void OnEnable()
    {
        foreach (var spawn in AgentSpawns.GetComponentsInChildren<Transform>())
        {
            if (spawn == AgentSpawns)
                continue;
            var agent = Instantiate(AgentPrefab).GetComponent<AgentController>();
            agent.transform.SetParent(AgentParent);
            agent.transform.position = spawn.position;
            agent.transform.rotation = spawn.rotation;
            agent.owner = this;
            _agents.Add(agent);
        }
    }
    
    private void RecalculateMass()
    {
        // tiles
        if (_allTiles == null)
        {
            _allTiles = new Tile[Width, Height];
            for (int x = 0; x < Width; ++x)
            {
                var column = transform.GetChild(x);
                for (int y = 0; y < Height; ++y)
                {
                    var tile = column.GetChild(y).GetComponent<Tile>();
                    _allTiles[x, y] = tile;
                }
            }
        }

        // center of mass
        Vector3 centerOfMass = Vector3.zero;
        float massSum = 0;
        foreach(Tile t in _allTiles)
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
        COMPivotOffset.y = 0;
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
    
    public Tile ReplaceTile(Tile oldTile, Tile.TileType newType)
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
        _allTiles[x, y] = newTile;
        return newTile;
    }

    #endregion
    
    void FixedUpdate()
    {
        HighlightedTile = null;
        if (TiltController.MainCamera != null)
        {
            Ray ray = TiltController.MainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out _mouseHit);
            if (_mouseHit.collider != null)
            {
                HighlightedTile = _mouseHit.collider.GetComponent<Tile>();
            }
        }
        
        #region Editor
        if (Application.isEditor)
        {
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
                DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject, false);
                dirty = true;
            }
            if (dirty || transform.GetChild(0).childCount != Height)
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
                        DestroyImmediate(column.GetChild(column.childCount - 1).gameObject, false);
                    }
                }
                _allTiles = null;
            }

            if (Input.GetKey(KeyCode.P))
            {
                if (HighlightedTile != null && HighlightedTile.Type != TilePainter)
                    HighlightedTile = ReplaceTile(HighlightedTile, TilePainter);
            }
        }
        #endregion
        
        if (Input.GetMouseButton(0))
        {
            if (!_isSelecting)
            {
                _selectStart = Input.mousePosition;
            }
            _isSelecting = true;
        }
        else
        {
            if (_isSelecting)
            {
                _selectedAgents.Clear();
                Vector3 SelectEnd = Input.mousePosition;
                Vector2 min = Vector3.Min(_selectStart, SelectEnd);
                Vector2 max = Vector3.Max(_selectStart, SelectEnd);
                Rect box = new Rect(min, max - min);
                foreach (var agent in _agents)
                {
                    Vector2 screenpt = TiltController.MainCamera.WorldToScreenPoint(agent.transform.position);
                    if (box.Contains(screenpt))
                        _selectedAgents.Add(agent);
                }
            }
            _isSelecting = false;
        }

        if (_selectedAgents.Count > 0 && HighlightedTile != null && Input.GetMouseButton(1))
        {
            foreach (var agent in _selectedAgents)
            {
                agent.SetDestination(HighlightedTile);
            }
        }

        RecalculateMass();
    }
}
