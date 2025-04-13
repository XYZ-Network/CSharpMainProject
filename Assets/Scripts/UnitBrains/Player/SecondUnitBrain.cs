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
                return; // Прекращаем стрельбу при перегреве
            }

            // Увеличиваем температуру через метод
            IncreaseTemperature();

            // Создаем снаряды (количество = текущая температура)
            for (int i = 0; i < GetTemperature(); i++)
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
            // Получаем список всех достижимых целей, до которых юнит может добраться
            List<Vector2Int> result = GetReachableTargets();

            // Проверяем, есть ли вообще цели в списке
            // Если список пустой (Count == 0) или равен null (ничего не найдено), просто возвращаем его без изменений
            if (result == null || result.Count == 0)
                return result;

            // Предположим, что ближайшая цель — это самая первая в списке
            Vector2Int closestTarget = result[0];

            // Создаем переменную, в которой будем хранить минимальное расстояние до базы
            // Начинаем с максимально возможного значения, чтобы любое реальное расстояние было меньше
            float minDistance = float.MaxValue;

            // Перебираем все цели в списке
            foreach (var target in result)
            {
                // Вычисляем расстояние от текущей цели до нашей базы
                float distance = DistanceToOwnBase(target);

                // Сравниваем это расстояние с минимальным найденным ранее
                if (distance < minDistance)
                {
                    // Если текущее расстояние меньше — обновляем переменные:
                    // 1. Запоминаем новое минимальное расстояние
                    minDistance = distance;

                    // 2. Запоминаем эту цель как ближайшую
                    closestTarget = target;
                }
            }

            // Возвращаем новый список, в котором только одна цель — та, что ближе всех к базе
            return new List<Vector2Int> { closestTarget };
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