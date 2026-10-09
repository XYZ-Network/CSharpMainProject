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
            if (GetTemperature() >= overheatTemperature)//Проверяем нет ли перегрева.
            {
                return;
            }

            IncreaseTemperature();//Сразу увеличиваем перегрев на 1, чтобы не было 0 при температуре 0.

            for (int i = 0; i < GetTemperature(); i++)//Запускаем цикл определяющий кол. снарядом в зависимости от перегрева
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
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
            float minDistance = float.MaxValue; // Фиксируем максимальное расстояние
            Vector2Int? closestTarget = null; // Объявляем пустой вектор
            
            foreach (var target in result) // Прозодимся по списку векторов доступных целей
            {
                var currentEnemyDistance = DistanceToOwnBase(target); // Получаем расстояние доступной цели до нашей базы
                if (minDistance > currentEnemyDistance)
                {
                    minDistance = currentEnemyDistance;
                    closestTarget = target;
                }
            }
            result.Clear(); // чистим лист от всех значений

            if (closestTarget.HasValue) // Если значение в векторе closestTraget?
            {
                result.Add(closestTarget.Value); // Если значение имеется, то добавляем его в лист целей
            }
            return result; //Возвращаем цель

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