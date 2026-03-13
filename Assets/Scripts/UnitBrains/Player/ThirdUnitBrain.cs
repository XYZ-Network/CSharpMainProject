using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Model.Runtime.Projectiles;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float _maxTime = 1f;
    private bool Attack = false;
    private bool Move = false;
    private bool hasTargets = false;
    private float _startTime = 0f;

    public override Vector2Int GetNextStep()
    {
        if (Attack || Move)
            return unit.Pos;

        return base.GetNextStep();
    }

    protected override List<Vector2Int> SelectTargets()
    {
        var result = base.SelectTargets();
        hasTargets = result.Count > 0;
        if (!Attack || Move)
            result.Clear();

        return result;
    }

    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);

        if (Attack && !hasTargets && !Move ||
            !Attack && hasTargets && !Move)
        {
            Move = true;
            _startTime = time;
        }

        if (Move && time - _startTime >= _maxTime)
        {
            Attack = !Attack;
            Move = false;
        }
    }
}


