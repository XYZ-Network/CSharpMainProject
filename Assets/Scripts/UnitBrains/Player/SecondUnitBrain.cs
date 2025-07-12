using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;

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

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (_overheated)
            {
                UnityEngine.Debug.Log("overheated, cooling down...");
                return;
            }

            // Проверка на радиус атаки перед выстрелом
            if (!IsInAttackRange(forTarget))
            {
                return;
            }

            IncreaseTemperature();

            int bulletCount = GetTemperature();

            for (int i = 0; i < bulletCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> reachable = GetReachableTargets();

            if (reachable.Count > 0)
            {
                Vector2Int closest = reachable[0];
                float minDist = (closest - unit.Pos).sqrMagnitude;

                foreach (var pos in reachable)
                {
                    float dist = (pos - unit.Pos).sqrMagnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = pos;
                    }
                }

                return new List<Vector2Int> { closest };
            }

            
            foreach (var target in GetAllTargets())
            {
                return new List<Vector2Int> { target }; 
            }

            return new List<Vector2Int>();
        }

        public override Vector2Int GetNextStep()
        {
            var targets = SelectTargets();

            if (targets.Count == 0)
                return base.GetNextStep();

            var target = targets[0];

            if (IsTargetInRange(target))
                return unit.Pos; 

            
            return new DummyUnitPath(runtimeModel, unit.Pos, target).GetNextStepFrom(unit.Pos);
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += UnityEngine.Time.deltaTime;
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
            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }

        // проверка радиуса атаки
        private bool IsInAttackRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }
    }
}
