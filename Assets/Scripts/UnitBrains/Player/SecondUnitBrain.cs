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

        private Vector2Int? _target;
        private List<Vector2Int> unreachableTargets = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            unreachableTargets.Clear();

            var allTargets = GetAllTargets();

            Vector2Int closestTarget = Vector2Int.zero;
            float closestDistance = float.MaxValue;

            foreach (var target in allTargets)
            {
                float distanceToBase = DistanceToOwnBase(target);
                if (IsTargetInRange(target))
                {
                    result.Add(target);
                }
                else
                {
                    unreachableTargets.Add(target);
                }

                if (distanceToBase < closestDistance)
                {
                    closestDistance = distanceToBase;
                    closestTarget = target;
                }
            }

            if (result.Count == 0)
            {
                _target = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            }
            else
            {
                _target = closestTarget;
            }

            return result;
        }

        public override Vector2Int GetNextStep()
        {
            if (_target.HasValue)
            {
                var target = _target.Value;

                if (!IsTargetInRange(target))
                {
                    return unit.Pos.CalcNextStepTowards(target);
                }
            }

            return unit.Pos;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += deltaTime;
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
            return _overheated ? (int)OverheatTemperature : (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}