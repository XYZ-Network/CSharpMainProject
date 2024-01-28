using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> outOfTargets = new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            // Получаем текущую температуру и проверяем на перегрев. Если перегреты, завершаем выполнение метода return'ом
            int temperature = GetTemperature();
            //Debug.Log(temperature);

            if (temperature >= overheatTemperature)
            {
                return;
            }

            // Создаем снаряд и определяем цель
            var projectile = CreateProjectile(forTarget);

            // в зависимости от температуры, добавляем количество выстрелов для юнита.
            for (int i = 0; i <= temperature; i++)
            {

                //Debug.Log("Выстрел " + i);
                AddProjectileToList(projectile, intoList);
            }    

            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (outOfTargets.Any())
            {
                var outOfTargetsCount = outOfTargets.Count - 1;
                var target = CalcNextStepTowards(outOfTargets[outOfTargetsCount]);
                return target;
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> resultsTarget = new ();

            // Локальная переменная, содержащая ссылку на условную цель.
            Vector2Int currentTarget = Vector2Int.zero;
            // Локальная переменная, с значением минимального растояния.
            float minDistance = float.MaxValue; 

            // Находим ближайший вражеский юнит и определяем его целью.
            foreach (var resultTarget in GetAllTargets())
            {
                float resultTargetDistance = DistanceToOwnBase(resultTarget);

                if (IsTargetInRange(currentTarget))
                {
                    outOfTargets.Add(currentTarget);
                }
                else
                {
                    resultsTarget.Add(currentTarget);
                    resultsTarget.Add(runtimeModel.RoMap.Bases[Model.RuntimeModel.BotPlayerId]);
                }

                if (resultTargetDistance < minDistance)
                {
                    currentTarget = resultTarget;
                    minDistance = resultTargetDistance;
                }

            }

            // Записываем цель в результирующий список целей, если такая была найдена.
            if (minDistance < float.MaxValue)
            {
                resultsTarget.Add(currentTarget);
            }

            return resultsTarget;
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