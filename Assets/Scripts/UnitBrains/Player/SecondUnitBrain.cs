using System.Collections.Generic;
using Model.Runtime.Projectiles;
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
        private int _shotsFired = 0;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            int currentTemperature = GetTemperature();

            if (!_overheated)
            {
                IncreaseTemperature();

                for (int i = 0; i < _shotsFired + 1; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }

            _shotsFired++;

            if (_shotsFired >= 3)
            {
                _overheated = true;
                _cooldownTime = OverheatCooldown;
                _shotsFired = 0;
            }
        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetReachableTargets();

            if (result.Count == 0)
            {
                return result;
            }

            Vector2Int closestTarget = result[0];
            float closestDistance = DistanceToOwnBase(result[0]);
            foreach (Vector2Int target in result)
            {
                float distance = DistanceToOwnBase(target);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            result.Clear();
            result.Add(closestTarget);

            return result;
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
            if (_overheated)
                return (int)OverheatTemperature;
            else
                return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}


