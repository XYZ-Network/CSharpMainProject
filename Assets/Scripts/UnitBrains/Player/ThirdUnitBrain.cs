using Model;
using Model.Runtime.Projectiles;
using System.Collections;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEditor.Graphs;
using UnityEngine;
using Utilities;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";
    private const float OverheatTemperature = 3f;
    private const float OverheatCooldown = 2f;
    private const int _maxTargets = 3;

    private float _temperature = 0f;
    private float _cooldownTime = 0f;
    public float TransitionTime = 1f;

    private bool _overheated;
    private bool _move = true;
    private bool _shot = false;

    private static int _idUnit = 0;

    public int Id { get; private set; }

    public List<Vector2Int> OutOfRange = new();

    public ThirdUnitBrain()
    {
        Id = ++_idUnit;
    }


    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        float overheatTemperature = OverheatTemperature;
        float t = GetTemperature();
        IncreaseTemperature();
        if (t >= overheatTemperature)
        {
            return;
        }

        for (float i = GetTemperature(); i <= 3 && i > 0; i--)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
        IncreaseTemperature();

    }

    void Transition()
    {
         for (float Transition = 1f; Transition == TransitionTime; Transition++)
            {
                if (HasTargetsInRange())
            {
                Shot();
            }
                 else
            {
                Move();

            }
            }
    }

    void Shot()
    {
        {
            _shot = true;
            _move = false;
        }
    }

    void Move()
    {
        _shot = false;
        _move = true;
    }



    public override Vector2Int GetNextStep()
    {
        Vector2Int target;
        target = OutOfRange.Count > 0 ? OutOfRange[0] : unit.Pos;
        return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);

        //Vector2Int target = OutOfRange[0];
        //Vector2Int nextPosition = Vector2Int.right;
        //if (OutOfRange.Count > 0 && !IsTargetInRange(target))
        //{
        //    return unit.Pos.CalcNextStepTowards(target);
        //}

        //else
        //{
        //    return unit.Pos;
        //}
        //return base.GetNextStep();
    }

    protected override List<Vector2Int> SelectTargets()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int TargetPosition;
        OutOfRange.Clear();

        foreach (Vector2Int target in GetAllTargets())
        {
            OutOfRange.Add(target);
        }


        if (OutOfRange.Count == 0)
        {
            if (_move == true)
            {
                Vector2Int baseEnemyID = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                OutOfRange.Add(baseEnemyID);
            }
        }
        else
        {
            SortByDistanceToOwnBase(OutOfRange);

            int targetIndex = Id % _maxTargets;

            if (targetIndex < OutOfRange.Count)
            {
                TargetPosition = OutOfRange[targetIndex];
            }
            else
            {
                TargetPosition = OutOfRange[OutOfRange.Count - 1];

            }
            if (IsTargetInRange(TargetPosition))
            {
                result.Add(TargetPosition);
            }
        }
        return result;


    }



    public override void Update(float deltaTime, float time)
    {
        Transition();
        if (_overheated)
        {
            _cooldownTime += Time.deltaTime;
            float t = _cooldownTime / (OverheatCooldown / 10);
            _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
            if (t >= 1)
            {
                _cooldownTime = 0;
                _overheated = false;
            }
        }
    }

    private int GetTemperature()
    {
        if (_overheated) return (int)OverheatTemperature;
        else return (int)_temperature;
    }

    private void IncreaseTemperature()
    {
        _temperature += 1f;
        if (_temperature >= OverheatTemperature) _overheated = true;
    }

}
