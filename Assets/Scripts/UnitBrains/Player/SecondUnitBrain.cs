using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        private static int targetCounter { get; set; } = 0;
        private int unitNumber { get; set; } = GetAndIncreaseUnitCounter();
        private static int cleverSearchMaxTarget = 3;
        public override string TargetUnitName => "Cobra Commando";
        private List<Vector2Int> moveTarget = new List<Vector2Int>();
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private static int GetAndIncreaseUnitCounter()
        {
            return targetCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (moveTarget.Count == 0 || IsTargetInRange(moveTarget[0]))
            {
                return unit.Pos;
            }

            return unit.Pos.CalcNextStepTowards(moveTarget[0]);
        }

        public List<Vector2Int> SelectEnemyBaseAsTarget()
        {
            RuntimeModel runtimeModel = new RuntimeModel();
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            moveTarget.Add(enemyBase);
            return new List<Vector2Int> { enemyBase };
        }

        public Vector2Int FindNearestToBaseTarget(List<Vector2Int> attackTargets)
        {
            Vector2Int nearestToBaseTarget = attackTargets.FirstOrDefault();

            foreach (Vector2Int target in attackTargets)
            {
                if (DistanceToOwnBase(target) < DistanceToOwnBase(nearestToBaseTarget))
                {
                    nearestToBaseTarget = target;
                }
            }

            return nearestToBaseTarget;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            moveTarget.Clear();
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            SortByDistanceToOwnBase(allTargets);
            if (allTargets.Count() == 0)
            {
                return SelectEnemyBaseAsTarget();
            }

            int targetIndex = cleverSearchMaxTarget > allTargets.Count() ? unitNumber % allTargets.Count() : unitNumber % cleverSearchMaxTarget;
            Vector2Int target = allTargets[targetIndex];
            moveTarget.Add(target);

            return IsTargetInRange(target) ? new List<Vector2Int>() { target } : new List<Vector2Int>();
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