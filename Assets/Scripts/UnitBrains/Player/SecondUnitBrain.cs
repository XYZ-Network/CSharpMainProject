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

        private static int unitCounter = 0; // Приватное статическое поле 
        private int unitNumber; // Приватное поле номера юнита
        private const int MaxTargetsToConsider = 3; // Приватная константа для макс. кол-ва целей

        public SecondUnitBrain()
        {
            unitNumber = ++unitCounter; 
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            int currentTemperature = GetTemperature();
            if (currentTemperature >= (int)OverheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            int projectileCount = currentTemperature + 1;
            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetReachableTargets();
            if (result.Count > 0)
            {
             
                SortByDistanceToOwnBase(result);

                if (result.Count > MaxTargetsToConsider)
                {
                    result = result.GetRange(0, MaxTargetsToConsider);
                }

             
                int targetIndex = (unitNumber - 1) % result.Count;
                Vector2Int target = result[targetIndex];

                
                result.Clear();
                result.Add(target);
            }
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += deltaTime;
                float t = _cooldownTime / OverheatCooldown;
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
            {
                _overheated = true;
            }
        }

        private void SortByDistanceToOwnBase(List<Vector2Int> targets)
        {
            targets.Sort((a, b) => DistanceToOwnBase(a).CompareTo(DistanceToOwnBase(b)));
        }
    }
}
