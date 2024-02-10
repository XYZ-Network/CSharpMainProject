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
            float temp = GetTemperature();

            if (temp >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i <= temp; i++)
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

            float enemyWithMinDistanceToBaseValue = float.MaxValue;

            Vector2Int enemyUnitTargetPosition = Vector2Int.zero; // Переменная enemyUnitTarget изначально будет хранить нулевую позицию(x = 0, y = 0), с помощью цикла запишем самую близкую цель к базе в эту переменную

            foreach (Vector2Int res in result)
            {
                float enemyDistanceToBaseValue = DistanceToOwnBase(res);

                if (enemyDistanceToBaseValue < enemyWithMinDistanceToBaseValue)
                {
                    enemyWithMinDistanceToBaseValue = enemyDistanceToBaseValue;
                    enemyUnitTargetPosition = res; // В переменную enemyUnitTargetPosition записали самую близкую цель к базе
                }
            }

            result.Clear(); // Очистка списка позиций всех вражеских целей

            if (enemyWithMinDistanceToBaseValue < float.MaxValue) // Условие, если enemyWithMinDistanceToBaseValue меньше самому большому значению по float, добавь самую близкую цель для атаки
            {
                result.Add(enemyUnitTargetPosition);
            }
            return result;

        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}