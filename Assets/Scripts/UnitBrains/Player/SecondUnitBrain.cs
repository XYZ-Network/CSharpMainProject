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
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           

            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();

            float MinDistance = float.MinValue;
            Vector2Int NearestTarget = new();

            foreach (Vector2Int target in result)
            {
                if (DistanceToOwnBase(target) > MinDistance)
                {
                    MinDistance = DistanceToOwnBase(target);
                    NearestTarget = target;
                }
            }

            result.Clear();

            //С помошью вывода в консоль "Debug.Log(NearestTarget);" узнал что координаты по которым стреляют мои юнити это (0,0)(если они стреляют в воздух)
            if (MinDistance != float.MaxValue & NearestTarget != new Vector2Int(0,0))// решил добавить второе условие вместе с первым которое указал проверяющий, понимаю что это костыль, но не знаю как поменять код, чтобы не было такого
            {
                result.Add(NearestTarget);
            }

            return result;
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
    }
}