using Codice.Client.Common;
using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private bool _pause = false;
    private float _pauseTime = 0f;
    private int mode = 0;
    public override bool IsPause => _pause;
    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        var projectile = CreateProjectile(forTarget);
        AddProjectileToList(projectile, intoList);
    }
    public override Vector2Int GetNextStep()
    {
        if (mode >= 1 || _pause)
        {
            return unit.Pos;
        }
        else
        {
            return base.GetNextStep();
        }
    }
    protected override List<Vector2Int> SelectTargets()
    {
        if (mode == 0 || _pause)
        {
            return new List<Vector2Int>();
        }
        else
        {
            if (base.SelectTargets().Count > 0)
            {
                if (IsTargetInRange(base.SelectTargets()[0]))
                    return base.SelectTargets();
                else
                {
                    return new List<Vector2Int>();
                }
            }
            else
            {
                return new List<Vector2Int>();
            }
        }
    }
    public override void Update(float deltaTime, float time)
    {
        Debug.Log($"{unit.Config.AttackRange} * {unit.AttackRangeMultiplier} = {unit.Config.AttackRange * unit.AttackRangeMultiplier}, sqrt = {unit.Config.AttackRange * unit.AttackRangeMultiplier * unit.Config.AttackRange * unit.AttackRangeMultiplier}");
        if (_pause == true)
        {
            if (_pauseTime < 1f)
            {
                _pauseTime += deltaTime * 10;
            }
            else if (_pauseTime >= 1f)
            {
                _pause = false;
                _pauseTime = 0;
            }
        }

        int lastMode = mode;
        mode = base.SelectTargets().Count;

        if (lastMode != mode)
        {
            _pause = true;
        }
    }
}
