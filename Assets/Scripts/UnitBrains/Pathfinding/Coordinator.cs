using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

public class Coordinator
{
    public int PlayerUnitID = 0;
    public int EnemyUnitID = 1;
    private static int _count = 0;
    private IReadOnlyRuntimeModel _runtimemodel;
    private TimeUtil _timeutil;

    private Vector2Int _recTarget;
    private Vector2Int _recPoint;

    public Vector2Int RecTarget => _recTarget;
    public Vector2Int RecPoint => _recPoint;
    public int Count { get { return _count; } set { _count = value; } }

    public Coordinator(int PlayerUnit, RuntimeModel runtimemodel, TimeUtil timeutil) 
    {
        this.PlayerUnitID = PlayerUnit;
        EnemyUnitID = PlayerUnit == 0 ? 1 : 0;
        _runtimemodel = runtimemodel;
        _timeutil = timeutil;
        _timeutil.AddFixedUpdateAction(CoordinatorUpdate);
    }

    private void CoordinatorUpdate(float deltatime)
    {
        CalculateRecommendation();
    }
    private void CalculateRecommendation()
    {
        float middlePoint = (Vector2Int.Distance(_runtimemodel.RoMap.Bases[PlayerUnitID], _runtimemodel.RoMap.Bases[EnemyUnitID])) / 2;
        Vector2Int PosResult = new Vector2Int();
        Vector2Int HealthResult = new Vector2Int();
        float minDist = float.MaxValue;
        float minHealth = float.MaxValue;
        foreach ( var enemy in PlayerUnitID == 0 ? _runtimemodel.RoBotUnits : _runtimemodel.RoPlayerUnits)
        {
            float dist = Vector2Int.Distance(_runtimemodel.RoMap.Bases[PlayerUnitID], enemy.Pos);
            if( dist < minDist)
            {
                minDist = dist;
                PosResult = enemy.Pos;
            }
            if(enemy.Health < minHealth)
            {
                HealthResult = enemy.Pos;
            }
        }

        if(Vector2Int.Distance(_runtimemodel.RoMap.Bases[PlayerUnitID], PosResult) < middlePoint)
        {
            _recTarget = PosResult;
            _recPoint = _runtimemodel.RoMap.Bases[PlayerUnitID] + Vector2Int.right;
        }
        else
        { 
            _recTarget = HealthResult;
            _recPoint = PosResult;
        }
        Count = 0;

    }

}
