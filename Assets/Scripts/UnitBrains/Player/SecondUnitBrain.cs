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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int temperature = GetTemperature();
            if (temperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i <= temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = GetReachableTargets();
            Vector2Int distanceBasa = Vector2Int.zero;
            float min = float.MaxValue;


            foreach (Vector2Int distance in result)
            {
                float distanceToBase = DistanceToOwnBase(distance);
                if (distanceToBase < min)
                {
                    distanceBasa = distance;
                    min = distanceToBase;
                }
            }
            result.Clear();
            if (min < float.MaxValue) result.Add(distanceBasa);

            return result;

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