using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Example1Battle : SrpgPhaseBattle
{
    public SrpgMap myMap;

    public Sprite playerUnitSprite;
    public RuntimeAnimatorController playerUnitAnimCtrl;
    public Sprite enemyUnitSprite;
    public RuntimeAnimatorController enemyUnitAnimCtrl;

    public GameObject msgObj;
    public Text msgText;

    private Example1Unit _targetUnit;

    public override bool InitConfig()
    {
        _unitFitMode = UnitFitMode.Inside;
        _moveMethod = MoveMethod.AlongPath;
        _unitMoveFrame = 10;
        _unitMoveTime = 0.1f;

        return true;
    }

    public override bool InitMap()
    {
        _map = myMap;

        return true;
    }

    public override bool InitUnits()
    {
        GameObject playerUnitObj = Instantiate(unitPrefab, _unitLayer.transform);
        Example1Unit playerUnit = playerUnitObj.GetComponent<Example1Unit>();
        playerUnit.Init(playerUnitSprite, playerUnitAnimCtrl);
        AddUnit(playerUnit, 7, 5, 0.9f, true);

        playerUnitObj = Instantiate(unitPrefab, _unitLayer.transform);
        playerUnit = playerUnitObj.GetComponent<Example1Unit>();
        playerUnit.Init(playerUnitSprite, playerUnitAnimCtrl);
        AddUnit(playerUnit, 13, 5, 0.9f, true);

        GameObject enemyUnitObj = Instantiate(unitPrefab, _unitLayer.transform);
        Example1Unit enemyUnit = enemyUnitObj.GetComponent<Example1Unit>();
        enemyUnit.Init(enemyUnitSprite, enemyUnitAnimCtrl);
        AddUnit(enemyUnit, 7, 15, 0.9f, false);

        enemyUnitObj = Instantiate(unitPrefab, _unitLayer.transform);
        enemyUnit = enemyUnitObj.GetComponent<Example1Unit>();
        enemyUnit.Init(enemyUnitSprite, enemyUnitAnimCtrl);
        AddUnit(enemyUnit, 13, 15, 0.9f, false);

        return true;
    }

    public override void StartGame()
    {
        msgObj.SetActive(true);
        msgText.text = "Game Start!";

        StartCoroutine(StartGame_Coroutine());
    }

    IEnumerator StartGame_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);

        msgObj.SetActive(false);

        base.StartGame();
    }

    public override void StartPhase(bool playerPhase)
    {
        msgObj.SetActive(true);
        msgText.text = playerPhase ? "Player Phase" : "Enemy Phase";

        StartCoroutine(ChangePhase_Coroutine(playerPhase));
    }

    IEnumerator ChangePhase_Coroutine(bool playerPhase)
    {
        yield return new WaitForSeconds(0.5f);

        msgObj.SetActive(false);

        base.StartPhase(playerPhase);
    }

    public override void EndGame(bool playerWin)
    {
        base.EndGame(playerWin);

        msgObj.SetActive(true);
        msgText.text = playerWin ? "Player Win!" : "Player Lose!";

        StartCoroutine(EndGame_Coroutine());
    }

    IEnumerator EndGame_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);

        msgObj.SetActive(false);
    }

    protected override bool OnUnitClicked(object sender, EventArgs e)
    {
        if (!base.OnUnitClicked(sender, e))
            return false;

        Example1Unit clickedUnit = (Example1Unit)sender;
        if (clickedUnit == null)
        {
            Debug.LogError("unit is null");
            return false;
        }

        if (_activeUnit == null)
        {
            if (clickedUnit.player)
            {
                Debug.Assert(clickedUnit.State != Example1Unit.CustomState.ReadyToMove &&
                             clickedUnit.State != Example1Unit.CustomState.ReadyToAttack);

                if (clickedUnit.State == Example1Unit.CustomState.Idle)
                {
                    ChangeActiveUnit(clickedUnit);
                    clickedUnit.SetState(Example1Unit.CustomState.ReadyToMove);
                }
            }
            else
            {
                ChangeActiveUnit(clickedUnit);
            }
        }
        else if (_activeUnit == clickedUnit)
        {
            if (_activeUnitAnimating)
                return false;

            if(_activeUnit.player)
            {
                if(clickedUnit.State == Example1Unit.CustomState.ReadyToMove)
                {
                    clickedUnit.SetState(Example1Unit.CustomState.ReadyToAttack);
                    ClearActiveUnitCells();
                    CreateActiveUnitAttackCells();
                }
                else if(clickedUnit.State == Example1Unit.CustomState.ReadyToAttack)
                {
                    clickedUnit.SetState(Example1Unit.CustomState.End);
                    ClearActiveUnit();
                }
            }
        }
        else // myActiveUnit != null
        {
            if (_activeUnitAnimating)
                return false;

            if(_activeUnit.player)
            {
                Example1Unit myActiveUnit = (Example1Unit)_activeUnit;
                if (myActiveUnit.State == Example1Unit.CustomState.ReadyToMove)
                {
                    if (clickedUnit.player)
                        clickedUnit.SetState(Example1Unit.CustomState.ReadyToMove);
                    myActiveUnit.SetState(Example1Unit.CustomState.Idle);
                    ChangeActiveUnit(clickedUnit);
                }
                else if (myActiveUnit.State == Example1Unit.CustomState.ReadyToAttack)
                {
                    if (!clickedUnit.player)
                    {
                        if (SrpgGameUtils.CalcSimpleDistance(_activeUnit, clickedUnit) <= myActiveUnit.attackRange)
                            Attack(myActiveUnit, clickedUnit);
                    }
                }
            }
            else
            {
                clickedUnit.SetState(Example1Unit.CustomState.ReadyToMove);
                ChangeActiveUnit(clickedUnit);
            }
        }

        return true;
    }

    protected override bool OnCellClicked(object sender, EventArgs e)
    {
        if (!base.OnCellClicked(sender, e))
            return false;

        if (_activeUnit == null || !_activeUnit.player || _activeUnitAnimating)
            return false;

        Example1Cell cell = (Example1Cell)sender;
        if (cell == null)
        {
            Debug.LogError("cell is null");
            return false;
        }

        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;
        if (myActiveUnit.State == Example1Unit.CustomState.ReadyToMove)
            MoveActiveUnit(cell);

        return true;
    }

    protected override bool OnTileClicked(object sender, EventArgs e)
    {
        if (!base.OnTileClicked(sender, e))
            return false;

        if (_activeUnit == null || _activeUnitAnimating)
            return false;

        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;
        if(_activeUnit.player)
        {
            if (myActiveUnit.State == Example1Unit.CustomState.ReadyToMove)
            {
                myActiveUnit.SetState(Example1Unit.CustomState.Idle);
                ClearActiveUnit();
            }
        }
        else
        {
            ClearActiveUnit();
        }

        return true;
    }

    private void ChangeActiveUnit(Example1Unit newUnit)
    {
        _activeUnit = newUnit;
        ClearActiveUnitCells();
        CreateActiveUnitMoveCells();
    }

    private void CreateActiveUnitMoveCells()
    {
        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;

        int range = myActiveUnit.moveRange;
        for (int y = myActiveUnit.y - range; y <= myActiveUnit.y + range; y++)
        {
            if (y < 0 || y > _map.mapHeight - 1)
                continue;

            for (int x = myActiveUnit.x - range; x <= myActiveUnit.x + range; x++)
            {
                if (x < 0 || x > _map.mapWidth - 1)
                    continue;

                if (SrpgGameUtils.CalcSimpleDistance(x, y, myActiveUnit.x, myActiveUnit.y) > range)
                    continue;

                List<SrpgAStarNode> closedNodes = null;
                if (!MakeUnitMovePath_AStar(myActiveUnit, x, y, out closedNodes, range))
                    continue;

                Example1Cell cell = (Example1Cell)_map.InstantiateCellIntoLayer(_cellLayer, cellPrefab, x, y);
                if (myActiveUnit.player)
                    cell.SetStateToMovable();
                else
                    cell.SetEnemyStateToMovable();

                _activeUnitCells.Add(cell);
            }
        }
    }

    private void CreateActiveUnitAttackCells()
    {
        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;

        int range = myActiveUnit.attackRange;
        for (int y = myActiveUnit.y - range; y <= myActiveUnit.y + range; y++)
        {
            if (y < 0 || y > _map.mapHeight - 1)
                continue;

            for (int x = myActiveUnit.x - range; x <= myActiveUnit.x + range; x++)
            {
                if (x < 0 || x > _map.mapWidth - 1)
                    continue;

                int dist = SrpgGameUtils.CalcSimpleDistance(x, y, myActiveUnit.x, myActiveUnit.y);
                if (dist > range)
                    continue;

                Example1Cell cell = (Example1Cell)_map.InstantiateCellIntoLayer(_cellLayer, cellPrefab, x, y);
                cell.SetStateToAttackable();

                _activeUnitCells.Add(cell);
            }
        }
    }

    public override bool IsTileWalkable(SrpgTile tile)
    {
        SrpgPropertyImpl prop = null;
        if (tile.TryGetProperty("Walkable", out prop))
        {
            if (!prop.b)
                return false;
        }

        return true;
    }

    public override bool MakeUnitMovePath(SrpgUnit unit, int destX, int destY, out List<SrpgNode> path)
    {
        path = new List<SrpgNode>();

        Example1Unit myUnit = (Example1Unit)unit;
        List<SrpgAStarNode> closedNodes = null;
        if (!MakeUnitMovePath_AStar(myUnit, destX, destY, out closedNodes, myUnit.GetMaximumRange()))
            return false;

        path = closedNodes.Cast<SrpgNode>().ToList();

        return true;
    }

    public override void OnActiveUnitMoveEnd()
    {
        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;
        myActiveUnit.SetState(Example1Unit.CustomState.ReadyToAttack);
        CreateActiveUnitAttackCells();
    }

    private void Attack(Example1Unit attackUnit, Example1Unit damageUnit)
    {
        StartCoroutine(Attack_Coroutine(attackUnit, damageUnit));
    }

    IEnumerator Attack_Coroutine(Example1Unit attackUnit, Example1Unit damageUnit)
    {
        _activeUnitAnimating = true;

        Vector3 attackVec = damageUnit.transform.position - attackUnit.transform.position;
        Vector3 attackDir = attackVec.normalized;
        float attackMaxDist = attackVec.magnitude / 2.0f;
        Vector3 attackUnitOrigin = attackUnit.transform.position;

        int halfFrame = _unitMoveFrame / 2;
        float eachFrameTime = _unitMoveTime / _unitMoveFrame;
        for (int framei = 0; framei < _unitMoveFrame; framei++)
        {
            float ratio = (halfFrame - Math.Abs(halfFrame - framei)) / (float)halfFrame;
            if (ratio == 1.0f)
                StartCoroutine(Damage_Coroutine(attackUnit, damageUnit));
            float attackDist = attackMaxDist * ratio;
            attackUnit.transform.position = attackUnitOrigin + attackDir * attackDist;
            yield return new WaitForSeconds(eachFrameTime);
        }

        if((attackUnit.transform.position-attackUnitOrigin).magnitude > 0.0f)
            attackUnit.transform.position = attackUnitOrigin;

        _activeUnitAnimating = false;

        attackUnit.SetState(Example1Unit.CustomState.End);
        ClearActiveUnit();
    }

    IEnumerator Damage_Coroutine(Example1Unit attackUnit, Example1Unit damageUnit)
    {
        float damage = attackUnit.offence * UnityEngine.Random.Range(0.8f, 1.2f) -
            damageUnit.defence * UnityEngine.Random.Range(0.3f, 0.5f);
        damageUnit.GetDamage((int)damage);

        yield return new WaitForSeconds(_unitMoveTime / _unitMoveFrame * 2f);

        if (damageUnit.hp > 0)
        {
            damageUnit.BeginIdleAnimation();
        }
        else
        {
            if (damageUnit.player)
                _playerUnits.Remove(damageUnit);
            else
                _enemyUnits.Remove(damageUnit);
            Destroy(damageUnit.gameObject);
        }
    }

    public override void AI_ChooseEnemyUnit()
    {
        foreach (SrpgUnit unit in _enemyUnits)
        {
            if (!unit.IsEnd())
            {
                Example1Unit myUnit = (Example1Unit)unit;
                myUnit.SetState(Example1Unit.CustomState.ReadyToMove);
                ChangeActiveUnit(myUnit);
                break;
            }
        }
    }

    public override void AI_RunActiveUnit()
    {
        Example1Unit myActiveUnit = (Example1Unit)_activeUnit;

        if(myActiveUnit.State == Example1Unit.CustomState.ReadyToMove)
        {
            int minDist = int.MaxValue;
            _targetUnit = null;
            foreach (SrpgUnit unit in _playerUnits)
            {
                int dist = SrpgGameUtils.CalcSimpleDistance(myActiveUnit, unit);
                if (dist < minDist)
                {
                    minDist = dist;
                    _targetUnit = (Example1Unit)unit;
                }
            }

            if(minDist == 1)
            {
                myActiveUnit.SetState(Example1Unit.CustomState.ReadyToAttack);
                ClearActiveUnitCells();
                CreateActiveUnitAttackCells();
            }
            else
            {
                minDist = int.MaxValue;
                SrpgCell targetCell = null;
                foreach (SrpgCell cell in _activeUnitCells)
                {
                    int dist1 = SrpgGameUtils.CalcSimpleDistance(cell.x, cell.y, _activeUnit.x, _activeUnit.y);
                    int dist2 = SrpgGameUtils.CalcSimpleDistance(cell.x, cell.y, _targetUnit.x, _targetUnit.y);
                    int distSum = dist1 + dist2;
                    if (distSum < minDist)
                    {
                        minDist = distSum;
                        targetCell = cell;
                    }
                }

                MoveActiveUnit(targetCell);
            }
        }
        else if(myActiveUnit.State == Example1Unit.CustomState.ReadyToAttack)
        {
            int dist = SrpgGameUtils.CalcSimpleDistance(myActiveUnit, _targetUnit);
            if(dist <= myActiveUnit.attackRange)
            {
                Attack(myActiveUnit, _targetUnit);
            }
            else
            {
                myActiveUnit.SetState(Example1Unit.CustomState.End);
                ClearActiveUnit();
            }
        }
    }
}
