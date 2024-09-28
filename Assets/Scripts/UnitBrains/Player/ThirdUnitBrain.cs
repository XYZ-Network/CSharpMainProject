using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Player;
using UnityEngine;

public class ThirdUnitBrain : DefaultPlayerUnitBrain
{
    public override string TargetUnitName => "Ironclad Behemoth";

    // Константы перегрева и переключения
    private const float OverheatTemperature = 3f;
    private const float OverheatCooldown = 2f;
    private const int MaxTargets = 3;
    private const float TransitionTime = 1f; // Время переключения между режимами

    // Переменные состояния
    private float temperature = 0f;
    private float cooldownTime = 0f;
    private bool overheated = false;
    private bool isMoving = false;
    private bool isTransitioning = false;
    private float transitionTimer = 0f;

    private static int idUnit = 0;
    public int Id { get; private set; }
    public List<Vector2Int> OutOfRange = new List<Vector2Int>();

    public ThirdUnitBrain()
    {
        Id = ++idUnit;
    }

    protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
    {
        if (isTransitioning || isMoving) return; // Блокировка стрельбы

        if (GetTemperature() < OverheatTemperature)
        {
            for (int i = 0; i < Mathf.Min(3, (int)(OverheatTemperature - temperature)); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
        }
    }

    public override void Update(float deltaTime, float time)
    {
        base.Update(deltaTime, time);

        if (!isTransitioning)
        {
            Transition();
        }
        else
        {
            transitionTimer += deltaTime;
            if (transitionTimer >= TransitionTime)
            {
                isTransitioning = false;
                transitionTimer = 0f;
            }
        }

        if (overheated)
        {
            cooldownTime += deltaTime;
            float t = cooldownTime / OverheatCooldown;
            temperature = Mathf.Lerp(OverheatTemperature, 0, t);
            if (t >= 1)
            {
                cooldownTime = 0;
                overheated = false;
            }
        }
    }

    private void Transition()
    {
        if (HasTargetsInRange() && !isMoving)
        {
            isTransitioning = true; // Переход на стрельбу
            Shot();
        }
        else if (!HasTargetsInRange() && isMoving)
        {
            isTransitioning = true; // Переход на движение
            Move();
        }
        else
        {
            // Смена состояния
            isMoving = !isMoving;
        }
    }

    void Shot()
    {
        
        isMoving = false;
    }

    void Move()
    {
        
        isMoving = true;
    }

    public override Vector2Int GetNextStep()
    {
        if (isTransitioning) return unit.Pos;

        if (OutOfRange.Count > 0)
        {
            Vector2Int target = OutOfRange[0];
            Vector2Int direction = target - unit.Pos;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return unit.Pos + new Vector2Int((int)Mathf.Sign(direction.x), 0);
            }
            else
            {
                return unit.Pos + new Vector2Int(0, (int)Mathf.Sign(direction.y));
            }
        }

        return unit.Pos;
    }

    protected override List<Vector2Int> SelectTargets()
    {
        if (isTransitioning) return new List<Vector2Int>();

        List<Vector2Int> result = new List<Vector2Int>();
        OutOfRange.Clear();
        OutOfRange.AddRange(GetAllTargets());

        if (OutOfRange.Count == 0 && !isMoving)
        {
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            OutOfRange.Add(enemyBase);
        }
        else
        {
            SortByDistanceToOwnBase(OutOfRange);
            int targetIndex = Id % MaxTargets;
            Vector2Int targetPosition = (targetIndex < OutOfRange.Count) ? OutOfRange[targetIndex] : OutOfRange[OutOfRange.Count - 1];

            if (IsTargetInRange(targetPosition))
            {
                result.Add(targetPosition);
            }
        }

        return result;
    }

    private int GetTemperature()
    {
        return overheated ? (int)OverheatTemperature : (int)temperature;
    }

    private void IncreaseTemperature()
    {
        temperature += 1f;
        if (temperature >= OverheatTemperature) overheated = true;
    }
}
