using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
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
        private List<Vector2Int> _priorityTargets = new List<Vector2Int>();

        public static int unitСounter = 0;
        private int unitNumber;
        private const int maxTargetsCount = 3;

        public SecondUnitBrain()
        {
            unitNumber = unitСounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            if (GetTemperature() >= overheatTemperature)
                return;
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;
            targetPosition = _priorityTargets.Count > 0 ? _priorityTargets[0] : unit.Pos;
            return IsTargetInRange(targetPosition) ? unit.Pos : base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var iD = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.BotPlayerId;
            var baseCoords = runtimeModel.RoMap.Bases[iD];

            _priorityTargets.Clear();
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> reachableTargets = GetReachableTargets();
            List<Vector2Int> closestTargets = new List<Vector2Int>();

            SortByDistanceToOwnBase(allTargets);

            var closestCount = maxTargetsCount > allTargets.Count ? allTargets.Count : maxTargetsCount;
            closestTargets.AddRange(allTargets.GetRange(0, closestCount));

            var targetIndex = unitNumber % maxTargetsCount;
            var indexIsExist = targetIndex < closestTargets.Count && targetIndex > 0;
            if (indexIsExist)
            {
                _priorityTargets.Add(closestTargets[targetIndex]);
            }
            else if (closestTargets.Count > 0)
            {
                _priorityTargets.Add(closestTargets[0]);
            }
            else
            {
                _priorityTargets.Add(baseCoords);
            }

            return reachableTargets.Contains(_priorityTargets.LastOrDefault()) ? _priorityTargets : reachableTargets;
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