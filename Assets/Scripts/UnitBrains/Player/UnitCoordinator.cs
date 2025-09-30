using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utilities;
using static UnityEngine.GraphicsBuffer;

public class UnitCoordinator
{

    private readonly IReadOnlyRuntimeModel _runtimeModel; 
    private TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();  

    private Vector2Int _target; 
    private List<Vector2Int> _enemyUnitsPos; 
    private IEnumerable<IReadOnlyUnit> _enemyUnits;
    private int _counter;
    private int _resultIterationCounter;

    private int _playerID;
    private int _enemyID;

    public UnitCoordinator(IReadOnlyRuntimeModel runtimeModel, int forplayer, int enemyID)
    {
        _runtimeModel = runtimeModel;
        _playerID = forplayer;
        _enemyID = enemyID;

        _enemyUnits = GetAllEnemyUnits();
        _enemyUnitsPos = unitsToListUnitsPos(_enemyUnits);
        _timeUtil.AddFixedUpdateAction(UpdateThis);
        _counter = 0;
        _resultIterationCounter = 0;
        _target = new Vector2Int(0, 0);
    }

    private void UpdateThis(float number)
    {
        _enemyUnits = GetAllEnemyUnits();
        _enemyUnitsPos = unitsToListUnitsPos(_enemyUnits);
        SortByDistanceToOwnBase(_enemyUnitsPos);
    }

    private float DistanceToBase(Vector2Int fromPos, int Id) =>
            Vector2Int.Distance(fromPos, _runtimeModel.RoMap.Bases[Id]);

    private void SortByDistanceToOwnBase(List<Vector2Int> list)
    {
        int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToBase(a, _playerID);
            var distanceB = DistanceToBase(b, _playerID);
            return distanceA.CompareTo(distanceB);
        }

            list.Sort(CompareByDistanceToOwnBase);
    }

    private bool EnemiesAreCloseToOurBase(List<Vector2Int> enemyUnitsPosition)
    {
        foreach (var enemyUnit in enemyUnitsPosition)
        {
            float distanceToOwnBase = DistanceToBase(enemyUnit, _playerID);
            float distanceToEnemyBase = DistanceToBase(enemyUnit, _enemyID);
            if (distanceToOwnBase < distanceToEnemyBase)
                return true;
        }
        return false;
    }

    private IEnumerable<IReadOnlyUnit> GetAllEnemyUnits()
    {
        return _runtimeModel.RoUnits
            .Where(u => u.Config.IsPlayerUnit != true);
    }

    private List<Vector2Int> unitsToListUnitsPos(IEnumerable<IReadOnlyUnit> units)
    {
        List<Vector2Int> unitsList = new List<Vector2Int>();
        foreach (var enemyUnit in units)
        {
            unitsList.Add(enemyUnit.Pos);
        }
        return unitsList;
    }

    private Vector2Int Result(List<Vector2Int> Targets)
    {
        _target = Targets[0];
        _counter++;
        if (_counter >= 3 && Targets.Count() >= 2 + _resultIterationCounter)
        {
            _resultIterationCounter++;
            _target = Targets[0 + _resultIterationCounter];
            _counter = 0;
            return _target;
        }
        if (Targets.Count() <  2 + _resultIterationCounter)
            _resultIterationCounter = 0;
        return Targets[0];
    }
    private Vector2Int Result(IEnumerable<IReadOnlyUnit> UnitTargets)
    {
        List<Vector2Int> Targets = unitsToListUnitsPos(UnitTargets);
        _target = Targets[0];
        _counter++;
        if (_counter >= 3 && Targets.Count() >= 2 + _resultIterationCounter)
        {
            _resultIterationCounter++;
            _target = Targets[0 + _resultIterationCounter];
            _counter = 0;
            return _target;
        }
        if (Targets.Count() < 2 + _resultIterationCounter)
            _resultIterationCounter = 0;
        return Targets[0];
    }


    public Vector2Int GetTarget()
    {
        if (!_enemyUnitsPos.Any())
            return _runtimeModel.RoMap.Bases[_enemyID];

        if (EnemiesAreCloseToOurBase(_enemyUnitsPos))
        {
            _target = _enemyUnitsPos[0];
            _counter++;
            if (_counter >= 3)
            {
                if (_enemyUnitsPos.Count() >= 2)
                {
                    _target = _enemyUnitsPos[1];
                    _counter = 0;
                }
            }
            return Result(_enemyUnitsPos);
        }

        var sortedUnits = _enemyUnits.OrderBy(u => u.Health).ToList();
        return Result(sortedUnits);
    }

    public Vector2Int GetPoint()
    {
        if (!_enemyUnitsPos.Any())
            return _runtimeModel.RoMap.Bases[_enemyID];

        if (EnemiesAreCloseToOurBase(_enemyUnitsPos))
        {
            Vector2Int diff = _enemyUnitsPos[0] + _runtimeModel.RoMap.Bases[_playerID];
            Vector2Int stepDiff = diff.SignOrZero();
            Vector2Int result = _runtimeModel.RoMap.Bases[_playerID] + stepDiff;
            return result;
        }

        return _enemyUnitsPos[0];
    }
}
