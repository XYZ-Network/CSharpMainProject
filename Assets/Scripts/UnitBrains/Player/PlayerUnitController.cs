using Model;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using UnitBrains;

public class PlayerUnitController : IReadOnlyUnitController, IDisposable
{
    public Vector2Int RecommendedTarget { get; private set; }
    public Vector2Int RecommendedPoint { get; private set; }
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;
    private bool _onPlayerHalf;
    private float _attackRange;
    private UnitSorter _unitSorter;


    public PlayerUnitController() 
    { 
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _unitSorter = new UnitSorter();

        _timeUtil.AddFixedUpdateAction(UpdateRecomendations);
    }

    private void UpdateRecomendations(float deltaTime)
    {
        var botUnits = _runtimeModel.RoBotUnits.ToList();

        if (botUnits.Any())
        {
            CheckBorder();
            UpdateRecommendedTarget(botUnits);
            UpdateRecommendedPoint(botUnits);
            return;
        }

        var botBasePosition = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        RecommendedTarget = botBasePosition;
        RecommendedPoint = botBasePosition;
    }

    private void UpdateRecommendedTarget(List<IReadOnlyUnit> botUnits)
    {
        if (_onPlayerHalf)
        {
            _unitSorter.SortByDistanceToBase(botUnits, EBaseType.PlayerBase);
        }
        else
        {
            _unitSorter.SortByHealth(botUnits);
        }

        RecommendedTarget = botUnits.First().Pos;
    }


    private void UpdateRecommendedPoint(List<IReadOnlyUnit> botUnits)
    {
        if (_onPlayerHalf)
        {
            RecommendedPoint = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId] + Vector2Int.up;
        }
        else
        {
            _unitSorter.SortByDistanceToBase(botUnits, EBaseType.PlayerBase);
            _attackRange = GetUnitAttackRange();
            int x = botUnits.First().Pos.x;
            int y = botUnits.First().Pos.y - Mathf.FloorToInt(_attackRange);
            RecommendedPoint = new Vector2Int(x, y);
        }
    }

    private float GetUnitAttackRange()
    {
        var playerUnits = _runtimeModel.RoPlayerUnits.ToList();

        if (playerUnits.Any())
            return playerUnits.First().Config.AttackRange;

        return 1f;
    }


    private void CheckBorder()
    {
        foreach (var unit in _runtimeModel.RoBotUnits)
        {
            if (BorderIsCrossed(unit.Pos))
            {
                _onPlayerHalf = true;
                return;
            }
        }

        _onPlayerHalf = false;
    }


    private bool BorderIsCrossed(Vector2Int botPos)
    {
        int playerBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId].y;
        int botBaseY = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId].y;
        int pointsToBorder = (botBaseY - playerBaseY) / 2;

        return botBaseY - botPos.y > pointsToBorder;
    }

    public void Dispose()
    {
        _timeUtil.RemoveFixedUpdateAction(UpdateRecomendations);
    }
}
