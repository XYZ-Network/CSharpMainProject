using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        // Имя целевого юнита
        public override string TargetUnitName => "Cobra Commando";

        // Параметры перегрева
        private const float OverheatTemperature = 3f; 
        private const float OverheatCooldown = 2f; 
        private float _temperature = 0f; 
        private float _cooldownTime = 0f; 
        private bool _overheated; 
        private List<Vector2Int> _priorityTargets = new List<Vector2Int>(); 

        public static int unitСounter = 0; 
        private int unitNumber; 
        private const int maxTargetsCount = 3; 

        // Конструктор
        public SecondUnitBrain()
        {
            unitNumber = unitСounter++; // Увеличиваем счетчик юнитов при создании нового
        }

        // Генерация проекций для атаки на заданную цель
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            // Проверяем, не перегрета ли текущая система
            if (GetTemperature() >= overheatTemperature)
                return; 

            
            for (int i = 0; i <= GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget); 
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
        }

        // Получение следующего шага юнита
        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;
            targetPosition = _priorityTargets.Count > 0 ? _priorityTargets[0] : unit.Pos; 
            return IsTargetInRange(targetPosition) ? unit.Pos : base.GetNextStep(); 
        }

        // Выбор целей для атаки
        protected override List<Vector2Int> SelectTargets()
        {
            var iD = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.BotPlayerId; // Получение ID базы
            var baseCoords = runtimeModel.RoMap.Bases[iD]; // Координаты базы

            _priorityTargets.Clear(); // Очищаем список приоритетных целей
            List<Vector2Int> allTargets = GetAllTargets().ToList(); // Получаем все цели
            List<Vector2Int> reachableTargets = GetReachableTargets(); // Получаем доступные цели
            List<Vector2Int> closestTargets = new List<Vector2Int>(); // Список ближайших целей

            SortByDistanceToOwnBase(allTargets); // Сортировка целей по расстоянию до базы

            // Определяем количество ближайших целей
            var closestCount = maxTargetsCount > allTargets.Count ? allTargets.Count : maxTargetsCount;
            closestTargets.AddRange(allTargets.GetRange(0, closestCount)); 

            var targetIndex = unitNumber % maxTargetsCount; 
            var indexIsExist = targetIndex < closestTargets.Count && targetIndex > 0; 

            // Добавляем приоритетные цели
            if (indexIsExist)
            {
                _priorityTargets.Add(closestTargets[targetIndex]); // Добавление по индексу
            }
            else if (closestTargets.Count > 0)
            {
                _priorityTargets.Add(closestTargets[0]); // Добавление первой ближайшей цели
            }
            else
            {
                _priorityTargets.Add(baseCoords); // Если нет доступных целей, добавляем координаты базы
            }

            return reachableTargets.Contains(_priorityTargets.LastOrDefault()) ? _priorityTargets : reachableTargets; // Возврат доступных целей
        }

        // Обновление состояния юнита
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
            return _overheated ? (int)OverheatTemperature : (int)_temperature; 
        }

      
        private void IncreaseTemperature()
        {
            _temperature += 1f; 
            if (_temperature >= OverheatTemperature) _overheated = true; 
        }
    }
}