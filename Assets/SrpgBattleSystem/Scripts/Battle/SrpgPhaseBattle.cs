using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SrpgPhaseBattle : SrpgBattle
{
    private bool _playerPhase = false;
    //private int _phaseCount = 0;

    private bool phaseChanging = true;

    public override void StartGame()
    {
        base.StartGame();

        StartPhase(true);
    }

    public virtual void StartPhase(bool playerPhase)
    {
        _playerPhase = playerPhase;

        foreach (SrpgUnit unit in _playerUnits)
            unit.Enliven();
        foreach (SrpgUnit unit in _enemyUnits)
            unit.Enliven();

        phaseChanging = false;
    }

    void Update()
    {
        if (!_gameRunning)
            return;

        if (phaseChanging)
            return;

        if(_playerUnits.Count > 0 && _enemyUnits.Count == 0)
        {
            EndGame(true);
            return;
        }
        else if(_playerUnits.Count == 0 && _enemyUnits.Count > 0)
        {
            EndGame(false);
            return;
        }

        List<SrpgUnit> activeUnits = (_playerPhase ? _playerUnits : _enemyUnits);

        bool allUnitEnd = true;
        foreach (SrpgUnit unit in activeUnits)
        {
            if (!unit.IsEnd())
            {
                allUnitEnd = false;
                break;
            }
        }

        if (allUnitEnd)
        {
            phaseChanging = true;
            StartPhase(!_playerPhase);
        }

        // ai
        if(!_playerPhase)
        {
            if(_activeUnit != null)
            {
                if(!_activeUnitAnimating)
                    AI_RunActiveUnit();
            }
            else
            {
                AI_ChooseEnemyUnit();
            }
        }
    }

    protected override bool OnUnitClicked(object sender, EventArgs e)
    {
        if (!_playerPhase || !base.OnUnitClicked(sender, e))
            return false;
        return true;
    }

    protected override bool OnCellClicked(object sender, EventArgs e)
    {
        if (!_playerPhase || !base.OnCellClicked(sender, e))
            return false;
        return true;
    }

    protected override bool OnTileClicked(object sender, EventArgs e)
    {
        if (!_playerPhase || !base.OnTileClicked(sender, e))
            return false;
        return true;
    }

    public abstract void AI_ChooseEnemyUnit();
}
