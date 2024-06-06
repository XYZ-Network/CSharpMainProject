using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using Unity.VisualScripting;
using UnityEngine;

public enum UnitState
{
    Move,
    Attack
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private float _timer = 0f;
    private float _actionDelayTime = 0.1f;
    private UnitState _state = UnitState.Move;
    private bool _changeState;

    public override Vector2Int GetNextStep() // выбор ячейки для передвижения
    {
        Vector2Int position = base.GetNextStep();

        if (position == unit.Pos)
        {
            if (_state == UnitState.Move)
                _changeState = true;

            _state = UnitState.Attack;
        }
        else
        {
            if (_state == UnitState.Attack)
                _changeState = true;

            _state = UnitState.Move;
        }

        return _changeState ? unit.Pos : position;
    }

    public override void Update(float deltaTime, float time)
    {
        if (_changeState)
        {
            _timer += Time.deltaTime;

            if (_timer > _actionDelayTime)
            {
                _timer = 0f;
                _changeState = false;
            }
                 
        }

        base.Update(deltaTime, time);
    }

    protected override List<Vector2Int> SelectTargets() // выбор целей для атаки
    {
        if (_changeState)
            return new List<Vector2Int>();

        if(_state == UnitState.Attack)
            return base.SelectTargets();

        return new List<Vector2Int>();
    }

}
