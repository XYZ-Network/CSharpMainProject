using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float ChangeCooldown = 1f;
    private ActionType _currentAction = ActionType.Movement;
    private bool _isCooldown = false;
    private float _cooldownTimer = 0f;
    
    private enum ActionType
    {
        Movement,
        Attack
    }
    
    
    public override Vector2Int GetNextStep()
    {
        if (_currentAction == ActionType.Movement && !_isCooldown)
        {
            return base.GetNextStep();
        }

        return unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (_currentAction == ActionType.Attack && !_isCooldown)
        {
            return base.SelectTargets();
        }

        return new List<Vector2Int>();
    }

    public override void Update(float deltaTime, float time)
    {
        if (_isCooldown)
        {
            _cooldownTimer += Time.deltaTime;

            if (_cooldownTimer > ChangeCooldown)
            {
                _cooldownTimer = 0f;
                _isCooldown = false;
            }
        }
        else
        {
            if (_currentAction == ActionType.Attack && !HasTargetsInRange())
            {
                _isCooldown = true;
                _currentAction = ActionType.Movement;
            }
            else if(_currentAction == ActionType.Movement && HasTargetsInRange())
            {
                _isCooldown = true;
                _currentAction = ActionType.Attack;
            }
        }
    }
}
