using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
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

        private Vector2Int? _movementTarget = null;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            
            float overheatTemperature = OverheatTemperature;
            float temperature = GetTemperature();

            if (temperature >= overheatTemperature)
            {
                return;
            }

            
            for (int i = 0; i <= temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();

            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_movementTarget == null || IsTargetInRange(_movementTarget.Value))
                return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_movementTarget.Value);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            _movementTarget = null;
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets().ToList();

            Vector2Int? primaryTarget = null;

            if (allTargets.Any())
            {
                primaryTarget = allTargets
                    .OrderBy(target => CalculateDistanceToTargetBase(target))
                    .First();
            }
            else
            {
                var enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                primaryTarget = runtimeModel.RoMap.Bases[enemyBaseId];
            }
            if (primaryTarget.HasValue)
            {
                if (IsTargetInRange(primaryTarget.Value))
                {
                    result.Add(primaryTarget.Value);
                }
                else
                {
                    _movementTarget = primaryTarget.Value;
                }
            }

            return result;
        }

        private float CalculateDistanceToTargetBase(Vector2Int position)
        {
            var ownerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
            var ownBasePos = runtimeModel.RoMap.Bases[ownerId];

            return Vector2Int.Distance(position, ownBasePos);
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
    }
}