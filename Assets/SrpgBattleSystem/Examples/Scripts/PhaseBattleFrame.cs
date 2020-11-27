using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseBattleFrame : SrpgPhaseBattle
{
    public override bool InitConfig()
    {
        // if need
        //_unitFitMode = ...;
        //_moveMethod = ...;
        //_unitMoveFrame = ...;
        //_unitMoveTime = ...;

        return true;
    }

    public override bool InitMap()
    {
        // load map
        //_map = ...;

        return true;
    }

    public override bool InitUnits()
    {
        // load unit data like sprite, animation, position, fit ratio, etc...
        //_playerUnits.Add(...);
        //_enemyUnits.Add(...);

        return true;
    }

    public override void StartGame()
    {
        //...

        base.StartGame();
    }

    public override void StartPhase(bool playerPhase)
    {
        //...

        base.StartPhase(playerPhase);
    }

    public override void EndGame(bool playerWin)
    {
        base.EndGame(playerWin);

        //...
    }

    // Priority Rank 1
    protected override bool OnUnitClicked(object sender, EventArgs e)
    {
        if (!base.OnUnitClicked(sender, e))
            return false;

        // unit state management

        return true;
    }

    // Priority Rank 2
    protected override bool OnCellClicked(object sender, EventArgs e)
    {
        if (!base.OnCellClicked(sender, e))
            return false;

        // unit state management

        return true;
    }

    // Priority Rank 3
    protected override bool OnTileClicked(object sender, EventArgs e)
    {
        if (!base.OnTileClicked(sender, e))
            return false;

        // unit state management

        return false;
    }

    public override void OnActiveUnitMoveEnd()
    {
        // when unit move animation is end
    }

    public override void AI_ChooseEnemyUnit()
    {
        // choose an enemy unit as active unit to be acted by ai
    }

    public override void AI_RunActiveUnit()
    {
        // ai logic with active unit
    }
}
