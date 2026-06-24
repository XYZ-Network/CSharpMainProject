using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

public class SecondUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Cobra Commando";

    private const float OverheatTemperature = 3f;
    private const float OverheatCooldown = 2f;

    private float _temperature = 0f;
    private float _cooldownTime = 0f;
    private bool _overheated;

    private Vector2Int _currentTarget;


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (GetTemperature() >= OverheatTemperature)
            return;

        float temperature = GetTemperature();

        for (int i = 0; i <= temperature; i++)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }

        IncreaseTemperature();
    }


    public override Vector2Int GetNextStep()
    {
        
        if (IsTargetInRange(_currentTarget))
            return unit.Pos;

       
        return unit.Pos.CalcNextStepTowards(_currentTarget);
    }


    protected override List<Vector2Int> SelectTargets()
    {
        var result = new List<Vector2Int>();
        var targets = GetAllTargets().ToList();

        
        if (targets.Count == 0)
        {
            _currentTarget =
                runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId
                ];

            result.Add(_currentTarget);
            return result;
        }

        
        Vector2Int nearest = Vector2Int.zero;
        float minDistance = float.MaxValue;

        foreach (var t in targets)
        {
            float distance = DistanceToOwnBase(t);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = t;
            }
        }

        
        _currentTarget = nearest;

        
        if (IsTargetInRange(nearest))
            result.Add(nearest);

        return result;
    }


    public override void Update(float deltaTime, float time)
    {
        if (_overheated)
        {
            _cooldownTime += deltaTime;

            float t = _cooldownTime / (OverheatCooldown / 10f);
            _temperature = Mathf.Lerp(OverheatTemperature, 0, t);

            if (t >= 1f)
            {
                _cooldownTime = 0;
                _overheated = false;
            }
        }
    }


    private int GetTemperature()
    {
        return _overheated ? (int)OverheatTemperature : (int)_temperature;
    }


    private void IncreaseTemperature()
    {
        _temperature += 1f;

        if (_temperature >= OverheatTemperature)
            _overheated = true;
    }
}