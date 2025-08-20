using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private float _changeStateTime = 0.4f;
    private float _currentStateTime = 0f;
    private bool _isChanging = false;
    private enum UnitState
    {
        Move,
        Attack
    }
    private UnitState _currentState = UnitState.Move;

    public override void Update(float deltaTime, float time)
    {
        if (_isChanging)
        {
            Debug.Log("check IsChanging true");
            _currentStateTime += deltaTime;
            Debug.Log("_currentStateTime = " + _currentStateTime);
            if (_currentStateTime > _changeStateTime)
            {
                Debug.Log("time exeed changeTime");
                ChangeState();
            }
        }
        else
        {
            Debug.Log("HasTargetsInRange() = " + HasTargetsInRange());
            if (HasTargetsInRange() && _currentState == UnitState.Move)
            {
                _isChanging = true;

                _currentStateTime = 0f;
            }
            if (!HasTargetsInRange() && _currentState == UnitState.Attack)
            {
                _isChanging = true;
                _currentStateTime = 0f;
            }
        }
    }

    private void ChangeState()
    {
        Debug.Log("enter the ChangeState");
        if (_currentState == UnitState.Attack && _isChanging)
        {
            Debug.Log("toMove");
            _currentState = UnitState.Move;
            _isChanging = false;
        }
        if (_currentState == UnitState.Move && _isChanging)
        {
            Debug.Log("toAttack");
            _currentState = UnitState.Attack;
            _isChanging = false;
        }


    }

    public override Vector2Int GetNextStep()
    {
        if (_currentState == UnitState.Attack)
        {
            return unit.Pos;
        }
        return base.GetNextStep();
    }
    protected override List<Vector2Int> SelectTargets()
    {
        if (_currentState == UnitState.Move)
        {
            return new List<Vector2Int>();
        }
        return base.SelectTargets();
    }
}
