using Model;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    bool canAttack = false;
    private BaseUnitPath _activePath = null;

    public override string TargetUnitName => "Ironclad Behemoth";

    protected override List<Vector2Int> SelectTargets()
    {
        var result = new List<Vector2Int>();
        if (canAttack)
        {
            result = GetReachableTargets();
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1);
        }
        return result;
    }

    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);
    }

    public override Vector2Int GetNextStep()
    {
        if (HasTargetsInRange())
        {
            canAttack = true;

            return unit.Pos;
        }
      
        canAttack = false;

        var target = runtimeModel.RoMap.Bases[
            IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

        _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
        return _activePath.GetNextStepFrom(unit.Pos);
    }
}
