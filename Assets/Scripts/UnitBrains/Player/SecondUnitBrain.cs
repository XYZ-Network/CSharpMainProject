using System;
using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private List<Vector2Int> TargetsOutReach = new List<Vector2Int>();
        private const int MaxTargets = 3;
        private static int unitCounter = 0;
        private int unitNumber;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            int currentTemperature = GetTemperature();

            if (currentTemperature >= OverheatTemperature)
                return;

            IncreaseTemperature();

            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        //public override Vector2Int GetNextStep()
        //{
        //    Vector2Int currentPos = unit.Pos;

        //    if (TargetsOutReach.Count > 0)
        //    {
        //        Vector2Int nearestEnemy = FindNearestEnemy(currentPos);
        //        if (IsTargetInRange(nearestEnemy))
        //            return currentPos;
        //        else
        //            return currentPos.CalcNextStepTowards(nearestEnemy);
        //    }
        //    else
        //    {
        //        Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
        //        if (IsTargetInRange(enemyBase))
        //            return currentPos;
        //        else
        //            return currentPos.CalcNextStepTowards(enemyBase);
        //    }
        //}


        private Vector2Int FindNearestEnemy(Vector2Int currentPosition)
        {
            Vector2Int nearestEnemy = TargetsOutReach[0];
            float minDistance = Vector2Int.Distance(currentPosition, nearestEnemy);

            foreach (Vector2Int enemyPos in TargetsOutReach)
            {
                float distance = Vector2Int.Distance(currentPosition, enemyPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemyPos;
                }
            }
            return nearestEnemy;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> targetsForAttack = new List<Vector2Int>();
            TargetsOutReach.Clear();

            foreach (var enemyTarget in GetAllTargets())
            {
                TargetsOutReach.Add(enemyTarget);
            }

            if (TargetsOutReach.Count == 0)
            {
                TargetsOutReach.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            SortByDistanceToOwnBase(TargetsOutReach);

            unitNumber = unitCounter % MaxTargets;

            int targetIndex = Mathf.Min(unitNumber, TargetsOutReach.Count - 1);
            Vector2Int target = TargetsOutReach[targetIndex];

            if (IsTargetInRange(target))
            {
                targetsForAttack.Add(target);
            }
            return targetsForAttack;
        }

        public override void Update(float deltaTime, float time)
        {
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
}