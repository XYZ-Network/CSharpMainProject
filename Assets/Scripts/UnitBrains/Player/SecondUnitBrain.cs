using System.Collections.Generic;
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

            // Проверка не перегрето ли оружие
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            
            //Увеличиваем количество снарядов с каждым выстрелом
            int WarmProjectiles = GetTemperature();
            for (int i = 0; i <= WarmProjectiles; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
           
            }

            //Поднимаем температуру после выстрела
            IncreaseTemperature();

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            List<Vector2Int> result = GetReachableTargets();

            //Если целей больше, чем 1, то ищем ближайшую цель
            if (result.Count > 1)
            {
                
                //Задаём максимальную дистанцию, чтобы относительно её найти ближайшую цель
                float nearestTargetDistance = float.MaxValue;
                    Vector2Int nearestTarget = Vector2Int.zero;

                //Узнаем, какая ближайшая цель
                foreach (Vector2Int target in result)
                {
                    //С помощью внешнего метода получаем дистанцию до ближайшей цели
                    float currentTargetDistance = DistanceToOwnBase(target);

                    //Обозначаем ближайшую цель
                    if (nearestTargetDistance > currentTargetDistance)
                    {
                        nearestTargetDistance = currentTargetDistance;
                        nearestTarget = target;
                    }
                }
                //Очищаем список перед добавлением цели
                result.Clear();
                //Добавляем цель в список
                result.Add(nearestTarget);

            }
            
            //Возвращаем список с целью
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