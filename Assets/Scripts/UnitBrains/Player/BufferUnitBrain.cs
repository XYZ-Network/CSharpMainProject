using Model;
using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View; 

public class BufferUnitBrain : BaseUnitBrain
{
    public override string TargetUnitName => "BufferUnit";
    public override bool IsPlayerUnitBrain => true;

    private const float buff_cd = 3f;
    private const float stop_beforebuff = 0.5f;
    private const float stop_afterbuff = 0.5f;

    private float _nextBuffTime = 0f;
    private float _buffStopStartTime = 0f;
    private float _buffStopEndTime = 0f;
    private BuffState _state = BuffState.Moving;
    private Unit _targetAlly = null;

    private enum BuffState
    {
        Moving,
        StoppingBeforeBuff,
        ApplyingBuff,
        StoppingAfterBuff
    }

    private BaseUnitPath _activePath = null;
    public override BaseUnitPath ActivePath => _activePath;

    public override void Update(float deltaTime, float time)
    {
        switch (_state)
        {
            case BuffState.Moving:
                if (time >= _nextBuffTime)
                {
                    var ally = FindAllyToBuff();
                    if (ally != null)
                    {
                        _targetAlly = ally;
                        _state = BuffState.StoppingBeforeBuff;
                        _buffStopStartTime = time;
                        _buffStopEndTime = time + stop_beforebuff;
                    }
                }
                break;

            case BuffState.StoppingBeforeBuff:
                if (time >= _buffStopEndTime)
                {
                    ApplyBuffToTarget();
                    _state = BuffState.StoppingAfterBuff;
                    _buffStopEndTime = time + stop_afterbuff;
                }
                break;

            case BuffState.StoppingAfterBuff:
                if (time >= _buffStopEndTime)
                {
                    _state = BuffState.Moving;
                    _nextBuffTime = time + buff_cd;
                    _targetAlly = null;
                }
                break;
        }
    }

    public override Vector2Int GetNextStep()
    {
        if (_state != BuffState.Moving)
        {
            return unit.Pos;
        }

        var target = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);
        return _activePath.GetNextStepFrom(unit.Pos);
    }

    protected override List<Vector2Int> SelectTargets()
    {
        return new List<Vector2Int>();
    }

    private Unit FindAllyToBuff()
    {
        var buffSystem = ServiceLocator.Get<BuffSystem>();
        var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

        foreach (var otherUnit in runtimeModel.RoUnits)
        {
            if (otherUnit == unit) continue;
            if (otherUnit.Config.Name == "BufferUnit") continue;
            if (otherUnit.Config.IsPlayerUnit != unit.Config.IsPlayerUnit) continue;

            var diff = otherUnit.Pos - unit.Pos;
            if (diff.sqrMagnitude > attackRangeSqr) continue;

            var allyUnit = otherUnit as Unit;
            if (allyUnit == null) continue;

            // Проверка — нет ли уже активного баффа атаки
            if (!buffSystem.HasActiveBuff<IBuff>(allyUnit))
            {
                Debug.Log($"[BufferUnit] цель баффа: {allyUnit.Config.Name}");
                return allyUnit;
            }
        }

        Debug.Log("[BufferUnit] некого баффать");
        return null;
    }

    private void ApplyBuffToTarget()
    {
        if (_targetAlly == null) return;

        var buffSystem = ServiceLocator.Get<BuffSystem>();

        IBuff buff;
        if (_targetAlly.Config.Name == "Cobra Commando")
            buff = new DoubleShotBuff<Unit>(5f);
        else if (_targetAlly.Config.Name == "Ironclad Behemoth")
            buff = new AttackRangeBuff<Unit>(5f, 2f);
        else
            buff = new AttackSpeedBuff<Unit>(5f, 1.5f);

        buffSystem.AddBuff(_targetAlly, buff);

        var vfxView = Object.FindObjectOfType<VFXView>();
        if (vfxView != null)
            vfxView.PlayVFX(_targetAlly.Pos, VFXView.VFXType.BuffApplied);

        Debug.Log($"[BufferUnit] бафнул {_targetAlly.Config.Name}: {buff.GetDescription()}");
    }
}