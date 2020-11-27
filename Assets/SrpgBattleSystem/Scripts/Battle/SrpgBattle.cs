using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SrpgBattle : MonoBehaviour
{
    #region enumerations
    public enum UnitFitMode
    {
        Unknown = 0,
        Inside,
        Width,
        Height
    }

    public enum MoveMethod
    {
        Unknown = 0,
        AlongPath,
        Straight
    }
    #endregion


    #region prefabs
    public GameObject cellPrefab;
    public GameObject unitPrefab;
    #endregion


    #region fields
    #region configuration
    protected UnitFitMode _unitFitMode = UnitFitMode.Inside;
    protected MoveMethod _moveMethod = MoveMethod.AlongPath;
    protected int _unitMoveFrame = 10;
    protected float _unitMoveTime = 0.1f;
    #endregion

    #region map & layer
    protected SrpgMap _map;
    protected SrpgLayer _groundLayer;
    protected SrpgLayer _unitLayer;
    protected SrpgLayer _cellLayer;
    #endregion

    #region units
    protected List<SrpgUnit> _playerUnits = new List<SrpgUnit>();
    protected List<SrpgUnit> _enemyUnits = new List<SrpgUnit>();
    #endregion

    protected SrpgUnit _activeUnit = null; // can be player unit or enemy unit
    protected List<SrpgCell> _activeUnitCells = new List<SrpgCell>();
    protected bool _activeUnitAnimating = false; // doing animation like moving, attacking, spelling, etc..

    protected bool _gameRunning = false;
    #endregion


    #region properties
    public UnitFitMode unitFitMode { get { return _unitFitMode; } }
    public MoveMethod moveMethod { get { return _moveMethod; } }
    public int unitMoveFrame { get { return _unitMoveFrame; } }
    public float unitMoveTime { get { return _unitMoveTime; } }

    public SrpgMap map { get { return _map; } }
    public SrpgLayer groundLayer { get { return _groundLayer; } }
    public SrpgLayer unitLayer { get { return _unitLayer; } }

    public List<SrpgUnit> playerUnits { get { return _playerUnits; } }
    public List<SrpgUnit> enemyUnits { get { return _enemyUnits; } }

    public SrpgUnit activeUnit { get { return _activeUnit; } set { _activeUnit = value; } }
    public List<SrpgCell> activeUnitCells { get { return _activeUnitCells; } }
    public bool activeUnitAnimating { get { return _activeUnitAnimating; } }

    public bool gameRunning { get { return _gameRunning; } set { _gameRunning = value; } }
    #endregion


    #region abstract method
    public abstract bool InitConfig();
    public abstract bool InitMap();
    public abstract bool InitUnits();

    public abstract void OnActiveUnitMoveEnd();

    public abstract void AI_RunActiveUnit();
    #endregion


    void Start()
    {
        //----- init configuration and check -----
        if (!InitConfig())
            return;

        if(_unitFitMode == UnitFitMode.Unknown)
        {
            Debug.LogError("unitFitMode is unknown");
            return;
        }
        if (_moveMethod == MoveMethod.Unknown)
        {
            Debug.LogError("moveMethod is unknown");
            return;
        }

        //----- init map & layers and check -----
        if (!InitMap())
            return;
        if(_map == null)
        {
            Debug.LogError("map is null");
            return;
        }

        List<SrpgLayer> layers = _map.GetSortedLayers();
        if(layers.Count == 0)
        {
            Debug.LogError("no layers");
            return;
        }

        if (_groundLayer == null)
            _groundLayer = layers[layers.Count - 1];

        // unit layer & cell layer z index 수정 필요
        if (_unitLayer == null)
            _unitLayer = _map.CreateLayer("Units");

        // magic number 5!?
        int cellLayerZ = _unitLayer.zIndexStart - 5;
        _cellLayer = _map.CreateLayer("Cells", false, cellLayerZ);

        foreach(SrpgLayer layer in layers)
        {
            if (layer.touchable)
            {
                SrpgTile[] tiles = layer.GetComponentsInChildren<SrpgTile>();
                foreach(SrpgTile tile in tiles)
                    tile.clicked += OnTileClicked;
            }
        }

        //----- init units and check -----
        if (!InitUnits())
            return;
        Debug.Assert(_playerUnits.Count > 0 && _enemyUnits.Count > 0);

        StartGame();
    }

    public void AddUnit(SrpgUnit unit, int x, int y, float fitRatio, bool player)
    {
        _map.LocateUnitFirst(_unitLayer, unit, x, y, fitRatio, _unitFitMode);
        unit.player = player;
        unit.clicked += OnUnitClicked;

        if (player)
            _playerUnits.Add(unit);
        else
            _enemyUnits.Add(unit);
    }

    public virtual void StartGame()
    {
        _gameRunning = true;
    }

    public virtual void EndGame(bool playerWin)
    {
        _gameRunning = false;
    }

    protected virtual bool OnUnitClicked(object sender, EventArgs e)
    {
        if (!_gameRunning)
            return false;
        return true;
    }

    protected virtual bool OnCellClicked(object sender, EventArgs e)
    {
        if (!_gameRunning)
            return false;
        return true;
    }

    protected virtual bool OnTileClicked(object sender, EventArgs e)
    {
        if (!_gameRunning)
            return false;

        SrpgTile tile = (SrpgTile)sender;
        if(tile == null)
        {
            Debug.LogError("tile is null");
            return false;
        }

        foreach (SrpgUnit unit in _playerUnits)
        {
            if (tile.x == unit.x && tile.y == unit.y)
            {
                OnUnitClicked(unit, e);
                return false;
            }
        }
        foreach (SrpgUnit unit in _enemyUnits)
        {
            if (tile.x == unit.x && tile.y == unit.y)
            {
                OnUnitClicked(unit, e);
                return false;
            }
        }

        foreach (SrpgCell cell in _activeUnitCells)
        {
            if (tile.x == cell.x && tile.y == cell.y)
            {
                OnCellClicked(cell, e);
                return false;
            }
        }

        return true;
    }

    public virtual void ClearActiveUnit()
    {
        _activeUnit = null;
        ClearActiveUnitCells();
    }

    public void ClearActiveUnitCells()
    {
        foreach (SrpgCell cell in _activeUnitCells)
            Destroy(cell.gameObject);

        _activeUnitCells.Clear();
    }

    public bool IsTherePlayerUnitAt(int x, int y, SrpgUnit self = null)
    {
        foreach (SrpgUnit unit in _playerUnits)
        {
            if (self == unit)
                continue;

            if (unit.x == x && unit.y == y)
                return true;
        }

        return false;
    }

    public bool IsTherePlayerUnitAt(SrpgCell cell, SrpgUnit self = null)
    {
        return IsTherePlayerUnitAt(cell.x, cell.y, self);
    }

    public bool IsThereEnemyUnitAt(int x, int y, SrpgUnit self = null)
    {
        foreach(SrpgUnit unit in _enemyUnits)
        {
            if (self == unit)
                continue;

            if (unit.x == x && unit.y == y)
                return true;
        }

        return false;
    }

    public bool IsThereEnemyUnitAt(SrpgCell cell, SrpgUnit self = null)
    {
        return IsThereEnemyUnitAt(cell.x, cell.y);
    }

    public virtual bool IsTileWalkable(SrpgTile tile)
    {
        return true;
    }

    public virtual bool MakeUnitMovePath(SrpgUnit unit, int destX, int destY, out List<SrpgNode> path)
    {
        path = new List<SrpgNode>();

        List<SrpgAStarNode> closedNodes = null;
        if (!MakeUnitMovePath_AStar(unit, destX, destY, out closedNodes))
            return false;

        path = closedNodes.Cast<SrpgNode>().ToList();

        return true;
    }

    protected bool MakeUnitMovePath_AStar(SrpgUnit unit, int destX, int destY,
        out List<SrpgAStarNode> closedNodes, int limitRange = 10000)
    {
        closedNodes = new List<SrpgAStarNode>();

        if (_groundLayer == null)
        {
            Debug.LogError("groundLayer is null");
            return false;
        }

        int point = 10;

        List<SrpgAStarNode> openNodes = new List<SrpgAStarNode>();

        // 1) Add the starting square (or node) to the open list. 
        openNodes.Add(new SrpgAStarNode(unit.x, unit.y, 0));

        // 2) Repeat the following:
        while (openNodes.Count > 0)
        {
            // a) Look for the lowest F cost square on the open list. We refer to this as the current square.
            openNodes.Sort();
            SrpgAStarNode currNode = openNodes[openNodes.Count - 1];
            openNodes.RemoveAt(openNodes.Count - 1);

            // b) Switch it to the closed list.
            closedNodes.Add(currNode);

            // check out of unit-range
            if (closedNodes.Count > limitRange + 1)
                return false;

            // check found path
            if (currNode.x == destX && currNode.y == destY)
                return true;

            // get adjacent tiles
            List<SrpgTile> adjacentTiles = _map.GetAdjacentTiles(_groundLayer, currNode.x, currNode.y);

            // c) For each of the adjacent to this current square …
            foreach (SrpgTile adjaTile in adjacentTiles)
            {
                SrpgAStarNode adjaNode = new SrpgAStarNode(adjaTile.x, adjaTile.y);

                // If it is not walkable or if it is on the closed list, ignore it. Otherwise do the following.
                if (closedNodes.Contains(adjaNode))
                    continue;

                if (unit.player)
                {
                    if (IsThereEnemyUnitAt(adjaTile.x, adjaTile.y))
                        continue;
                }
                else
                {
                    if (IsTherePlayerUnitAt(adjaTile.x, adjaTile.y))
                        continue;
                }

                if (!IsTileWalkable(adjaTile))
                    continue;

                int newGCost = currNode.GCost + point;

                SrpgAStarNode openNode = openNodes.Find(oi => (oi.x == adjaNode.x && oi.y == adjaNode.y));
                if (openNode != null)
                {
                    // If it is on the open list already, check to see if this path to that square is better, using G cost as the measure. A lower G cost means that this is a better path. If so, change the parent of the square to the current square, and recalculate the G and F scores of the square. If you are keeping your open list sorted by F score, you may need to resort the list to account for the change. 
                    if (newGCost < openNode.GCost)
                    {
                        openNode.GCost = newGCost;
                        openNode.Parent = currNode;
                    }
                }
                else
                {
                    // If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. Record the F, G, and H costs of the square.  
                    adjaNode.GCost = newGCost;
                    adjaNode.HCost = Math.Abs(adjaNode.x - destX) * point + Math.Abs(adjaNode.y - destY) * point;
                    adjaNode.Parent = currNode;
                    openNodes.Add(adjaNode);
                }
            }
        }

        return false;
    }

    public void MoveActiveUnit(SrpgCell cell)
    {
        MoveActiveUnit(cell.x, cell.y);
    }

    public void MoveActiveUnit(int x, int y)
    {
        if (_activeUnit == null)
        {
            Debug.LogError("active unit is null");
            return;
        }

        ClearActiveUnitCells();

        if (_moveMethod == MoveMethod.AlongPath)
        {
            List<SrpgNode> path = null;
            if (!MakeUnitMovePath(_activeUnit, x, y, out path))
            {
                Debug.LogError("Can't find a path");
                return;
            }

            StartCoroutine(MoveActiveUnitAlongPath(path));
        }
        else if (moveMethod == MoveMethod.Straight)
        {
            // call SrpgUnit.OnUnitMove(direction)
        }
    }

    protected virtual IEnumerator MoveActiveUnitAlongPath(List<SrpgNode> path)
    {
        _activeUnitAnimating = true;

        float eachFrameTime = _unitMoveTime / _unitMoveFrame;
        for (int movei = 0; movei < path.Count-1; movei++)
        {
            SrpgNode startNode = path[movei];
            SrpgNode destNode = path[movei+1];
            _activeUnit.BeginMoveAnimation(startNode, destNode);

            // 현재 위치 타일에서 목표 위치 타일로 이동
            Vector3 startPos = _map.CalcTilePosition(startNode.x, startNode.y);
            Vector3 destPos = _map.CalcTilePosition(destNode.x, destNode.y);
            Vector3 startToDest = destPos - startPos;
            for(int framei=0; framei<_unitMoveFrame; framei++)
            {
                _activeUnit.transform.position = _activeUnit.transform.position + startToDest.normalized * startToDest.magnitude / _unitMoveFrame;
                yield return new WaitForSeconds(eachFrameTime);
            }
            _activeUnit.x = destNode.x;
            _activeUnit.y = destNode.y;
        }

        _activeUnitAnimating = false;

        _activeUnit.BeginIdleAnimation();

        OnActiveUnitMoveEnd();
    }
}
