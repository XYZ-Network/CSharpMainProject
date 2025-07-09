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
        private static int _idCounter = 0;
        public const int maxClosestTargets = 3;
        public readonly int UnitId = _idCounter++;
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private Vector2Int? _moveTarget;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////
            if (_overheated)
            {
                return;
            }

            int currentTemp = GetTemperature();
            
            for (int i = 0; i <= currentTemp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);                
            }
            
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (!_moveTarget.HasValue || IsTargetInRange(_moveTarget.Value))
            {
                _activePath = null;
                return unit.Pos;
            }
        
            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, _moveTarget.Value, IsPlayerUnitBrain);
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            List<Vector2Int> closestTargets = FindTargetsClosestToBase().ToList();

            _moveTarget = closestTargets.Count > 0
                ? closestTargets[UnitId % closestTargets.Count]
                : null;
            
            List<Vector2Int> result = new();

            if (_moveTarget.HasValue && IsTargetInRange(_moveTarget.Value))
            {
                result.Add(_moveTarget.Value);
            }

            return result;
            ///////////////////////////////////////
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

        private IEnumerable<Vector2Int> FindTargetsClosestToBase()
        {
            var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            
            var targets = GetAllTargets()
                .Where(t => t != enemyBase) // Add enemy base as a target only if there are no other units
                .OrderBy(DistanceToOwnBase)
                .Take(maxClosestTargets);

            return targets.DefaultIfEmpty(enemyBase);
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