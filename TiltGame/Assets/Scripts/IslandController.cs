using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IslandController : MonoBehaviour
{
    public Transform AgentParent;
    public Transform AgentSpawns;
    public Transform Pivot;
    public Transform COMIndicator;
    public Transform CornerAndMeshParent;

    [Space]

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

    public GameObject GroundStraightPrefab;
    public GameObject GroundInnerPrefab;
    public GameObject GroundOuterPrefab;
    public GameObject GroundFlatPrefab;

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
        CornerAndMeshParent.position = Vector3.up;
        //CornerAndMeshParent.position = transform.position;

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
    
    private void RefreshTiles()
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
    }

    private void RecalculateMass()
    {
        RefreshTiles();

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
    
    private void RefreshCorners()
    {
        var meshes = CornerAndMeshParent.GetComponentsInChildren<Transform>();
        foreach (var mesh in meshes)
            if (mesh != CornerAndMeshParent)
                DestroyImmediate(mesh.gameObject, false);

        RefreshTiles();

        for (int x = 1; x < Width; ++x)
        {
            for (int y = 1; y < Height; ++y)
            {
                Tile nw = _allTiles[x - 1, y - 1];
                Tile sw = _allTiles[x - 1, y];
                Tile se = _allTiles[x, y];
                Tile ne = _allTiles[x, y - 1];
                RefreshSingleCorner(nw, sw, se, ne);
            }
        }
    }

    #region Corner logic
    
    private Transform MakeOuter(CornerRotation rotation)
    {
        Vector3 offset = Vector3.zero;
        float angle = 0;
        switch (rotation)
        {
            case CornerRotation.ThreeQ: angle = 270; offset.Set(0, 0, 0.5f); break;
            case CornerRotation.ZeroQ: angle = 0; offset.Set(0,0,0); break;
            case CornerRotation.OneQ: angle = 90; offset.Set(-0.5f, 0, 0); break;
            case CornerRotation.TwoQ: angle = 180; offset.Set(-0.5f, 0, 0.5f); break;
        }

        Transform t = Instantiate(GroundOuterPrefab).transform;
        t.Rotate(Vector3.up, angle, Space.Self);
        t.SetParent(CornerAndMeshParent);
        t.localPosition = offset;
        return t;
    }

    private Transform MakeInner(CornerRotation rotation)
    {
        Vector3 offset = Vector3.zero;
        float angle = 0;
        switch (rotation)
        {
            case CornerRotation.OneQ: angle = 270; offset.Set(0, 0, 0.5f); break;
            case CornerRotation.TwoQ: angle = 0; offset.Set(0, 0, 0); break;
            case CornerRotation.ThreeQ: angle = 90; offset.Set(-0.5f, 0, 0); break;
            case CornerRotation.ZeroQ: angle = 180; offset.Set(-0.5f, 0, 0.5f); break;
        }

        Transform t = Instantiate(GroundInnerPrefab).transform;
        t.Rotate(Vector3.up, angle, Space.Self);
        t.SetParent(CornerAndMeshParent);
        t.localPosition = offset;
        return t;
    }

    private Transform MakeStraight(CornerRotation rotation)
    {
        Vector3 offset = Vector3.zero;
        float angle = 0;
        switch (rotation)
        {
            case CornerRotation.TwoQ: angle = 270; offset.Set(0, 0, 0.5f); break;
            case CornerRotation.OneQ: angle = 0; offset.Set(0, 0, 0); break;
            case CornerRotation.ZeroQ: angle = 90; offset.Set(-0.5f, 0, 0); break;
            case CornerRotation.ThreeQ: angle = 180; offset.Set(-0.5f, 0, 0.5f); break;
        }

        Transform t = Instantiate(GroundStraightPrefab).transform;
        t.Rotate(Vector3.up, angle, Space.Self);
        t.SetParent(CornerAndMeshParent);
        t.localPosition = offset;
        return t;
    }

    private Transform MakeFlat()
    {
        Vector3 offset = Vector3.zero;
        float angle = 0;

        Transform t = Instantiate(GroundFlatPrefab).transform;
        t.Rotate(Vector3.up, angle, Space.Self);
        t.SetParent(CornerAndMeshParent);
        t.localPosition = offset;
        return t;
    }

    private enum CornerRotation
    {
        ThreeQ, 
        ZeroQ, 
        OneQ, 
        TwoQ
    }

    private void RefreshSingleCorner(Tile nw, Tile sw, Tile se, Tile ne)
    {
        Transform nwCorner = null;
        Transform swCorner = null;
        Transform seCorner = null;
        Transform neCorner = null;

        bool nwSolid = nw.Type != Tile.TileType.Air;
        bool neSolid = ne.Type != Tile.TileType.Air;
        bool seSolid = se.Type != Tile.TileType.Air;
        bool swSolid = sw.Type != Tile.TileType.Air;

        if (nwSolid) { 
            if (neSolid) { 
                if (seSolid) { 
                    if (swSolid) {
                        // _ _
                        //|0|0|
                        //|0|0|
                        swCorner = MakeFlat();
                        seCorner = MakeFlat();
                        nwCorner = MakeFlat();
                        neCorner = MakeFlat();
                    }
                    else {
                        // _ _
                        //|0|0|
                        //|_|0|
                        swCorner = MakeInner(CornerRotation.TwoQ);
                        seCorner = MakeFlat();
                        nwCorner = MakeFlat();
                        neCorner = MakeFlat();
                    }
                } else { 
                    if (swSolid) {
                        // _ _
                        //|0|0|
                        //|0|_|
                        swCorner = MakeFlat();
                        seCorner = MakeInner(CornerRotation.ThreeQ);
                        nwCorner = MakeFlat();
                        neCorner = MakeFlat();
                    }
                    else {
                        // _ _
                        //|0|0|
                        //|_|_|
                        nwCorner = MakeStraight(CornerRotation.OneQ);
                        neCorner = MakeStraight(CornerRotation.OneQ);
                    }
                }
            } else {  
                if (seSolid) { 
                    if (swSolid) {
                        // _ _
                        //|0|_|
                        //|0|0|
                        swCorner = MakeFlat();
                        seCorner = MakeFlat();
                        nwCorner = MakeFlat();
                        neCorner = MakeInner(CornerRotation.ZeroQ);
                    }
                    else {
                        // _ _
                        //|0|_|
                        //|_|0|
                        swCorner = MakeInner(CornerRotation.TwoQ);
                        seCorner = MakeFlat();
                        nwCorner = MakeFlat();
                        neCorner = MakeInner(CornerRotation.ZeroQ);
                    }
                } else { 
                    if (swSolid) {
                        // _ _
                        //|0|_|
                        //|0|_|
                        nwCorner = MakeStraight(CornerRotation.ZeroQ);
                        swCorner = MakeStraight(CornerRotation.ZeroQ);
                    }
                    else {
                        // _ _
                        //|0|_|
                        //|_|_|
                        nwCorner = MakeOuter(CornerRotation.OneQ);
                    }
                }
            }
        } else {  
            if (neSolid) { 
                if (seSolid) { 
                    if (swSolid) {
                        // _ _
                        //|_|0|
                        //|0|0|
                        swCorner = MakeFlat();
                        seCorner = MakeFlat();
                        nwCorner = MakeInner(CornerRotation.OneQ);
                        neCorner = MakeFlat();
                    }
                    else {
                        // _ _
                        //|_|0|
                        //|_|0|
                        neCorner = MakeStraight(CornerRotation.TwoQ);
                        seCorner = MakeStraight(CornerRotation.TwoQ);
                    }
                } else { 
                    if (swSolid) {
                        // _ _
                        //|_|0|
                        //|0|_|
                        swCorner = MakeFlat();
                        seCorner = MakeInner(CornerRotation.ThreeQ);
                        nwCorner = MakeInner(CornerRotation.OneQ);
                        neCorner = MakeFlat();
                    }
                    else {
                        // _ _
                        //|_|0|
                        //|_|_|
                        neCorner = MakeOuter(CornerRotation.ZeroQ);
                    }
                }
            } else {  
                if (seSolid) { 
                    if (swSolid) {
                        // _ _
                        //|_|_|
                        //|0|0|
                        swCorner = MakeStraight(CornerRotation.ThreeQ);
                        seCorner = MakeStraight(CornerRotation.ThreeQ);
                    }
                    else {
                        // _ _
                        //|_|_|
                        //|_|0|
                        seCorner = MakeOuter(CornerRotation.ThreeQ);
                    }
                } else { 
                    if (swSolid) {
                        // _ _
                        //|_|_|
                        //|0|_|
                        swCorner = MakeOuter(CornerRotation.TwoQ);
                    }
                    else {
                        // _ _
                        //|_|_|
                        //|_|_|
                    }
                }
            }
        }

        float small = 0.5f;
        float large = 1;
        Vector3 basis = nw.transform.localPosition;

        if (nwCorner != null) nwCorner.transform.Translate(basis + new Vector3(small, 0, small), Space.World);
        if (neCorner != null) neCorner.transform.Translate(basis + new Vector3(large, 0, small), Space.World);
        if (swCorner != null) swCorner.transform.Translate(basis + new Vector3(small, 0, large), Space.World);
        if (seCorner != null) seCorner.transform.Translate(basis + new Vector3(large, 0, large), Space.World);

        nw.SE = nwCorner == null ? null : nwCorner.GetComponent<TileCorner>();
        ne.SW = neCorner == null ? null : neCorner.GetComponent<TileCorner>();
        se.NW = seCorner == null ? null : seCorner.GetComponent<TileCorner>();
        sw.NE = swCorner == null ? null : swCorner.GetComponent<TileCorner>();
    }
    
    #endregion

    private Tile CreateNewTile(int x, int y, Tile.TileType type)
    {
        switch (type)
        {
            case Tile.TileType.Food:
                return Instantiate(FoodPrefab).GetComponent<Tile>();
            case Tile.TileType.Mountain:
                return Instantiate(MountainPrefab).GetComponent<Tile>();
            case Tile.TileType.Ground:
                return Instantiate(GroundPrefab).GetComponent<Tile>();
            default:
                return Instantiate(AirPrefab).GetComponent<Tile>();
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
        DestroyImmediate(oldTile.gameObject, false);
        var pos = newTile.transform.localPosition;
        pos.x = x;
        pos.z = y;
        newTile.transform.localPosition = pos;
        newTile.gameObject.name = "Tile " + y + ": " + newType.ToString();
        _allTiles[x, y] = newTile;
        RefreshCorners();
        //for (int i = x; i <= x + 1; ++i)
        //{
        //    if (i <= 0 || i >= Width)
        //        continue;
        //    for (int j = y; j <= y + 1; ++j)
        //    {
        //        if (j <= 0 || j >= Height)
        //            continue;
        //
        //        RefreshSingleCorner(
        //            _allTiles[i - 1, j - 1], 
        //            _allTiles[i - 1, j], 
        //            _allTiles[i, j], 
        //            _allTiles[i, j - 1]);
        //    }
        //}
        return newTile;
    }

    #endregion
    
    void FixedUpdate()
    {
        HighlightedTile = null;
        if (TiltController.MainCamera != null)
        {
            Ray ray = TiltController.MainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out _mouseHit);//, 1000, LayerMask.NameToLayer("Tile"));
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
                RefreshCorners();
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
