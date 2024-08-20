using Model;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class Coordinator 
{
    private Vector2Int recommendTarget;
    private Vector2Int recommendPos;
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;
    public Coordinator()
    {
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil.AddFixedUpdateAction(Update);
    }
    public Vector2Int getRecommendTarget()
    {
        return recommendTarget;
    }
    public Vector2Int getRecommendPos()
    {
        return recommendPos;
    }
    public void Update(float deltaTime)
    {
        int count = 0;
        int minHealth = 99999;
        float minDistance = 99999;
        Vector2Int PlayerBase = _runtimeModel.RoMap.Bases[0];

        var enemys = _runtimeModel.RoBotUnits;
        List<Vector2Int> targetsOverBase = new List<Vector2Int>();

        foreach(var enemy in enemys)
        {
            if (Math.Abs(enemy.Pos.y) - Math.Abs(PlayerBase.y) < 8) 
            {
                count++;
                targetsOverBase.Add(enemy.Pos);
            }
        }
        if (count > 0)
        {
            recommendPos.Set(PlayerBase.x + 1, PlayerBase.y);
            foreach (var enemy in targetsOverBase)
            {
                var distance = Vector2Int.Distance(enemy, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    recommendTarget = enemy;
                }
                
            }
        }

        else
        {
            foreach (var enemy in enemys)
            {
                if (enemy.Health < minHealth)
                {
                    minHealth = enemy.Health;
                    recommendTarget = enemy.Pos;
                }
            }

            foreach (var enemy in enemys)
            {
                var distance = Vector2Int.Distance(enemy.Pos, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    recommendPos.Set(enemy.Pos.x-1, enemy.Pos.y);
                }

            }
        }

        
    }
}
