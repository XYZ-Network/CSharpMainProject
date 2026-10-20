using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

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
        private List<Vector2Int> _unreachableTargets = new List<Vector2Int>();
        
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
            if (_unreachableTargets.Count == 0)// есть ли доступная цель
            {
                return unit.Pos; //стоим на месте
            }
            Vector2Int target = _unreachableTargets[0]; 
            if (IsTargetInRange(target)) // противник в зоне досягаемости?
            {
                return unit.Pos; // стоим на месте
            }
            
            return unit.Pos.CalcNextStepTowards(target);// идём к цели


        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            _unreachableTargets.Clear(); //Чистим список целей в не досягаемости
            List<Vector2Int> result = new List<Vector2Int>(); // Создаём пустой список целей для атаки
            Vector2Int? closestTarget = null; // Ближайщая цель
            IEnumerable<Vector2Int> allTargets = GetAllTargets();//Список всех целей
            float minDistance = float.MaxValue; // Фиксируем максимальное расстояние
            
            foreach (var target in allTargets) // Проходимся по списку всех целей
            {
                var currentEnemyDistance = DistanceToOwnBase(target); // Получаем расстояние всех целей до нашей базы
                if (minDistance > currentEnemyDistance)
                {
                    minDistance = currentEnemyDistance;
                    closestTarget = target;//Записываем ближайшую цель к базе
                }
            }

            if (closestTarget.HasValue) // Если ли цели есть?
            {
                _unreachableTargets.Add(closestTarget.Value); // Если значение имеется, то добавляем в лист цель к которой нужно двигаться 
                if (IsTargetInRange(closestTarget.Value)) // Проверяем доступна ли цель и записываем её в итоговую цель
                {
                    result.Add(closestTarget.Value);
                }
            }
            else
            {
                var idEnemy = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId; // определяем чей юнит
                Vector2Int basePosition = runtimeModel.RoMap.Bases[idEnemy]; // записываем позицию вражеской базы
                _unreachableTargets.Add(basePosition); // добавляем в список целей к которым нужно двигаться вражескую базу
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