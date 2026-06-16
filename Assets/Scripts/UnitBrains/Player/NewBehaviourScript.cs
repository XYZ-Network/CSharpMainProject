using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public enum LastAction
    {
        Step,
        Fire
    }

    public override string TargetUnitName => "Ironclad Behemoth";
    private LastAction lastAction = LastAction.Step;
    private float _actionCooldownTime = 0;

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (lastAction == LastAction.Step && _actionCooldownTime == 0)
        {
            _actionCooldownTime = 1;
            lastAction = LastAction.Fire;
        }

        if (_actionCooldownTime <= 0)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            lastAction = LastAction.Fire;
        }
        else {
            _actionCooldownTime -= Time.deltaTime;
            if (_actionCooldownTime < 0) { _actionCooldownTime = 0; }
        }
    }

    public override Vector2Int GetNextStep()
    {
        if (HasTargetsInRange())
            return unit.Pos;

        if (lastAction == LastAction.Fire && _actionCooldownTime == 0)
        {
            _actionCooldownTime = 1;
            lastAction = LastAction.Step;
        }

        if (_actionCooldownTime <= 0)
        {
            lastAction = LastAction.Step;
            return base.GetNextStep();
        }
        else
        {
            _actionCooldownTime -= Time.deltaTime;
            if (_actionCooldownTime < 0) { _actionCooldownTime = 0; }
            return unit.Pos;
        }
    }
}
