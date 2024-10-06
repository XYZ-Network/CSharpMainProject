using Codice.Client.Common;
using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private enum UnitStates
    {
        Moving,
        Attacking,
        Transition
    }
    private float _currentTransitionTime = 0f;
    private float transitionTime;
    private List<Vector2Int> TargetOutOfRange = new List<Vector2Int>();
    private UnitStates _currentState = UnitStates.Moving;
    private UnitStates _nextState = UnitStates.Moving;


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (_currentState == UnitStates.Attacking)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
    }


    public override void Update(float deltaTime, float time)
    {
        _currentTransitionTime = 0f;
        if (_currentState == UnitStates.Transition)
        {
            while (_currentTransitionTime < TransitionTime)
            {
                _currentTransitionTime += deltaTime;
            }
            _currentState = _nextState;
        }
    }

    protected override List<Vector2Int> SelectTargets()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        TargetOutOfRange.Clear();
        IEnumerable<Vector2Int> resultAsIE = GetAllTargets();
        result = resultAsIE.ToList();
        Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
        if (!result.Any())
            result.Add(enemyBase);
        SortByDistanceToOwnBase(result);

        Vector2Int enemy = new Vector2Int(0, 0);

        while (result.Count != 1)
        {
            result.RemoveAt(result.Count - 1);
        }

        enemy = result[0];
        result.Clear();
        if (IsTargetInRange(enemy))
        {
            if (_currentState != UnitStates.Attacking)
            {
                _nextState = UnitStates.Attacking;
                _currentState = UnitStates.Transition;
            }
            else
            {
                result.Add(enemy);
            }
        }
        else
        {
            if (_currentState != UnitStates.Moving)
            {
                _nextState = UnitStates.Moving;
                _currentState = UnitStates.Transition;
            }
            else
            {
                TargetOutOfRange.Add(enemy);
            }
        }
        return result;
    }

    public override Vector2Int GetNextStep()
    {
        Vector2Int position = unit.Pos;
        if ((_currentState == UnitStates.Moving) && TargetOutOfRange.Any())
        {
            foreach (var target in TargetOutOfRange)
            {
                position = unit.Pos;
                position = position.CalcNextStepTowards(target);
            }
            return position;
        }
        else
        {
            return unit.Pos;
        }
    }

    public float TransitionTime { get; private set; } = 1f;   //12000f
}
