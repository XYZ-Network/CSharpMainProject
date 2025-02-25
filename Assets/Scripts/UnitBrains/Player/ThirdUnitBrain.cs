using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

  enum UnitState
{
    Attack,
    Move
}   

  public class ThirdUnitBrain : DefaultPlayerUnitBrain
{

    public override string TargetUnitName => "Ironclad Behemoth";

    private UnitState _currentUnitState = UnitState.Move;
    
    private float stun = 0f;
    private float stunDuaration = 0.1f;
    
    private bool _isState;

    public override Vector2Int GetNextStep()
    {
        Vector2Int target = base.GetNextStep();

        if (target == unit.Pos)
        {
            if (_currentUnitState == UnitState.Move)
            {
                _isState = true;
            }

            _currentUnitState = UnitState.Attack;
        }
        else
        {
            if (_currentUnitState == UnitState.Attack)
            {
                _isState = true;
            }

            _currentUnitState = UnitState.Move;
        }

        return _isState ? unit.Pos : target;

    }
    protected override List<Vector2Int> SelectTargets()
    {
        if (_isState)
        {
            return new List<Vector2Int>();
        }

        if (_currentUnitState == UnitState.Attack)
        {
            return base.SelectTargets();
        }

        return new List<Vector2Int>();
    }
    public override void Update(float deltaTime, float time)
    {
        if (_isState)
        {
            stun += Time.deltaTime;

            if (stun >= stunDuaration)
            {
                stun = 0f;
                _isState = false;
            }
        }

        base.Update(deltaTime, time);
    }
}
