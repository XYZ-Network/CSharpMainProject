using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float _cooldownTime = 1f;
    private float _reloadTime;

    private bool _isCooldown = false;

    private ActionUnit _currentAction;

    private enum ActionUnit
    {
        Move,
        Attack
    }

    public override Vector2Int GetNextStep()
    {
        if (!_isCooldown && _currentAction == ActionUnit.Move)
        { 
            return base.GetNextStep();
        }
        else
        {
            return unit.Pos;
        }
    }


    protected override List<Vector2Int> SelectTargets()
    {
        if(!_isCooldown && _currentAction == ActionUnit.Attack)
        {
            return base.SelectTargets();
        }
        else
        {
            return new List<Vector2Int>();
        }
    }

    public override void Update(float deltaTime, float time)
    {
       
        if (_isCooldown)
        {
            _reloadTime += Time.deltaTime + 0.25f;
            Debug.Log("В юните " + deltaTime); 

            if (_reloadTime >= _cooldownTime)
            { 
                _reloadTime = 0f;
                _isCooldown = false;
            }
        }
        else
        {
            if (!HasTargetsInRange() && _currentAction == ActionUnit.Attack)
            {
                _isCooldown = true;
                _currentAction = ActionUnit.Move;
            }
            else if(HasTargetsInRange() && _currentAction == ActionUnit.Move)
            {
                _isCooldown = true;
                _currentAction = ActionUnit.Attack;
            }
        }
    }
}
