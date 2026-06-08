using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;


public enum UnitState
{
    Moving,
    Attacking,
    Switching
}

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    private UnitState _state = UnitState.Moving;
    private float _switchTimer = 0f;
    private UnitState _nextState;
    private UnitState desiredState;
    public override string TargetUnitName => "Ironclad Behemoth";

    public override void Update(float deltaTime, float time)
    {

        if (_state == UnitState.Switching)
        {
            _switchTimer += deltaTime;

            if (_switchTimer >= 1f)
            {
                _state = _nextState;
                _switchTimer = 0f;
            }
        }
    }
    public override Vector2Int GetNextStep()
    {
        desiredState = HasTargetsInRange()
            ? UnitState.Attacking
            : UnitState.Moving;

        if (_state != desiredState && _state != UnitState.Switching)
        {
            _nextState = desiredState;
            _state = UnitState.Switching;
            _switchTimer = 0f;
        }
        if (_state != UnitState.Moving)
            return unit.Pos;

            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

        _activePath = new DummyUnitPath(runtimeModel, unit.Pos, target);
        return _activePath.GetNextStepFrom(unit.Pos);
    }

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (_state != UnitState.Attacking)
            return;
        AddProjectileToList(CreateProjectile(forTarget), intoList);
    }
}


