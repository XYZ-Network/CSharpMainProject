using System.Collections;
using System.Collections.Generic;
using Model;
using System.Linq;
using Model.Runtime.Projectiles;
using Model.Runtime.StatusEffects;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using UnitBrains.Pathfinding;
using Model.Runtime.ReadOnly;

public class BufferUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Sky Buffer";

    private const float _buffDelayAfter = 0.25f;
    private const float _buffDelayBefore = 0.25f;
    private float _lastBuffTime = 0f;
    private float _beginBuffTime = 0f;

    private StatusEffects statusEffects => ServiceLocator.Get<StatusEffects>();

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        // this unit don't shut
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return new List<Vector2Int>();
        // this unit don't shut
    }

    public override void Update(float deltaTime, float time)
    {
        if ((_lastBuffTime + _buffDelayAfter) < time)
        {
            var reachableComrade = GetReachableComradeWithoutEffects();

            if (reachableComrade.Count() > 0)
            {
                _beginBuffTime += deltaTime;

                if (_beginBuffTime >= _buffDelayBefore)
                {
                    statusEffects.TryAddStatusEffect(reachableComrade[0], new AttackHastyEffect());
                    statusEffects.TryAddStatusEffect(reachableComrade[0], new MultiShotEffect());
                    statusEffects.TryAddStatusEffect(reachableComrade[0], new AttackRangeExpandEffect());
                    _lastBuffTime = time;
                    _beginBuffTime = 0;
                }
            }
        }
    }

    public override Vector2Int GetNextStep()
    {
        var forwardComrade = GetForwardComrade();

        var nextPos = unit.Pos;

        if (forwardComrade.Count() > 0 && !IsTargetInRange(forwardComrade[0]))
        {
            AStarUnitPath astarPath = new AStarUnitPath(runtimeModel, unit.Pos, forwardComrade[0], this);
            base.ActivePath = astarPath;

            nextPos = astarPath.GetNextStepFrom(unit.Pos);
        }

        return nextPos;
    }

    private List<int> GetReachableComradeWithoutEffects()
    {
        return runtimeModel.RoPlayerUnits
            .Where(u => u.Config.Name != this.TargetUnitName
                && statusEffects.HasStatusEffect(u.UnitId) == false
                && IsTargetInRange(u.Pos) == true)
            .Select(u => u.UnitId)
            .ToList();
        ;
    }

    private List<Vector2Int> GetForwardComrade()
    {
        return runtimeModel.RoPlayerUnits
            .Where(u => u.Config.Name != this.TargetUnitName)
            .OrderByDescending(u => DistanceToOwnBase(u.Pos))
            .Select(u => u.Pos)
            .ToList();
        ;
    }
}
