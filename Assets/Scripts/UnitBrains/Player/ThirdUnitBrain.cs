using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class ThirdUnitScript : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    public List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
    private static int idCounter = -1;
    private int id = idCounter++;
    private int MaxCount = 3;
    private BaseUnitPath _activePath = null;

    private float _switchCooldown = 2f; //1 second
    private float _switchTime = 0f;
    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (_actionSwitched)
            return; 
        AddProjectileToList(CreateProjectile(forTarget), intoList);
    }
    public override void Update(float deltaTime, float time)
    {
        
        if (_actionSwitched)
        {
            _switchTime += Time.deltaTime;
            float t = _switchTime / (_switchCooldown / 10);
            if (t >= 1)
            {
                _actionSwitched = false;
                _switchTime = 0;


            }
        }

    }
    public override Vector2Int GetNextStep()
    {
        if (HasTargetsInRange())
        {
            return unit.Pos;
        }
        _actionSwitched = true;
        var target = runtimeModel.RoMap.Bases[
            IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

        _activePath = new UnitBrains.Pathfinding.AdvancedUnitPath(runtimeModel, unit.Pos, target);
        return _activePath.GetNextStepFrom(unit.Pos);
    }

}
