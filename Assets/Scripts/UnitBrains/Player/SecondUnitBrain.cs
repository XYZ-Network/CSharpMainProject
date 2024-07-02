using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = GetReachableTargets();
            Vector2Int targetEn = new Vector2Int();
            float minD = float.MaxValue;

            if (result.Count == 0)
            {
                return new List<Vector2Int>();
            }

            foreach (var target in result)
            {
                float dis = DistanceToOwnBase(target);
                if (dis < minD)
                {
                    minD = dis;
                    targetEn = target;
                }
            }
            result.Clear();
            result.Add(targetEn);

            while (result.Count > 1)
            {

                result.RemoveAt(result.Count - 1);
            }
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