using System.Collections.Generic;
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
        private List<Vector2Int> Targets = new List<Vector2Int>();
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            int currentTemparature = GetTemperature();
            if (currentTemparature >= overheatTemperature) return;

            for (int i = 0; i <= currentTemparature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            bool isNecessaryMoveToTarget = Targets.Count > 0 && !IsTargetInRange(Targets[0]);

            return isNecessaryMoveToTarget ? unit.Pos.CalcNextStepTowards(Targets[0]) : unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            Targets.Clear();

            Vector2Int? nearestTargetToBase = GetNearestTargetToBase();
            Targets.Add(nearestTargetToBase.HasValue ? nearestTargetToBase.Value : GetEnemyTargetBase());

            return Targets;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
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
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }

        private Vector2Int? GetNearestTargetToBase()
        {
            Vector2Int? nearestTarget = null;

            float nearestDistance = float.MaxValue;
            foreach (Vector2Int target in GetAllTargets())
            {
                float distance = DistanceToOwnBase(target);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = target;
                }
            }

            return nearestTarget;
        }

        private Vector2Int GetEnemyTargetBase()
        {
            int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;

            return runtimeModel.RoMap.Bases[playerId];
        }
    }
}