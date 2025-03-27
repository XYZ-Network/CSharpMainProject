using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private bool _isAttacking = false;
    private bool _isMoving = true;
    private bool _stopped = false;
    private float _stopTime = 0f;
    private const float fixedDelay = 0.3f;


    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);

        if (_stopped)
        {
            _stopTime += Time.deltaTime;
            if (_stopTime >= fixedDelay)
            {
                _stopTime = 0f;
                _stopped = false;
            }
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        List<Vector2Int> targets = base.SelectTargets();
        if ((!_isMoving && _isAttacking) && !_stopped)
        {
            return targets;
        }

        targets.Clear();
        return targets;
    }

    public override Vector2Int GetNextStep()
    {
        var nextPosition = base.GetNextStep();
        if (nextPosition != unit.Pos)
        {
            if (_isAttacking && !_isMoving)
                _stopped = true;
            _isMoving = true;
            _isAttacking = false;
        }
        else
        {
            if (!_isAttacking && _isMoving)
                _stopped = true;
            _isMoving = false;
            _isAttacking = true;
        }

        if (_stopped)
            return unit.Pos;
        else
            return nextPosition;
    }
}
