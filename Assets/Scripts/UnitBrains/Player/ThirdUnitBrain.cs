using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    private const float _cooldownTime = 1f; // задержка в 1 сек
    private float _reloadTime; // перезагр действ

    private bool _isCooldown = false; //  false = перезагр // true = действие доступно

    private ActionUnit _currentAction;

    private enum ActionUnit 
    {
        Move,
        Attack
    }

    public override Vector2Int GetNextStep() // получаем следующий шаг 
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


    protected override List<Vector2Int> SelectTargets() // выбор цели 
    {
        if (!_isCooldown && _currentAction == ActionUnit.Attack)
        {
            return base.SelectTargets();
        }
        else
        {
            return new List<Vector2Int>();
        }
    }

    public override void Update(float deltaTime, float time) //обновление состояния 
    {

        if (_isCooldown) // проверка состояния перезарядки // _isCooldown=true, это означает, что объект не может выполнять действие
        {
            _reloadTime += Time.deltaTime + 0.25f;

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
            else if (HasTargetsInRange() && _currentAction == ActionUnit.Move)
            {
                _isCooldown = true;
                _currentAction = ActionUnit.Attack;
            }
        }
    }
}
