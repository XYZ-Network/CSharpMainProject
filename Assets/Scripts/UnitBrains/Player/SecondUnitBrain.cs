using System.Collections.Generic;
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
            // Seems like reachable targets check is done several times during step calculation.
            // Will it be better to cache result somewhere? Not sure about significant performance gain though
            // with such a small amount of units.
            return HasTargetsInRange() || !_moveTarget.HasValue
                ? unit.Pos
                : unit.Pos.CalcNextStepTowards(_moveTarget.Value);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            // `GetAllTargets()` also contains enemy's base as last element, so whe all
            // units are destroyed, we'll have it as move target
            _moveTarget = FindClosestToBase(GetAllTargets());

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

        private Vector2Int? FindClosestToBase(IEnumerable<Vector2Int> targets)
        {
            float minDist = float.MaxValue;
            Vector2Int? closestTarget = null;

            foreach (var targetPos in targets)
            {
                var targetDist = DistanceToOwnBase(targetPos);

                if (targetDist < minDist)
                {
                    minDist = targetDist;
                    closestTarget = targetPos;
                }
            }

            return closestTarget;
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