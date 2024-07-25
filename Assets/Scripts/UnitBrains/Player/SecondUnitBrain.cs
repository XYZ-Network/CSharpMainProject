using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using Vector2Int = UnityEngine.Vector2Int;

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
        
        public static int UnitID = 0; // Счетчик юниток
        public int UnitNumber = UnitID++; // Номер юнита который равен айди с инкрементом
        public int MaximumTargets = 3; // Максимальное количество целей
//tst
        private readonly List<Vector2Int> _currentTarget = new();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemeperature = GetTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (currentTemeperature >= overheatTemperature)
            {
                return;
            }
            else
            {
                for (int i = 0; i <= currentTemeperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                    Debug.Log(_temperature + 1);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;
            targetPosition = _currentTarget.Count > 0 ? _currentTarget[0] : unit.Pos;
            return IsTargetInRange(targetPosition) ? unit.Pos : unit.Pos.CalcNextStepTowards(targetPosition);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>(); // Пустой список для хранения целей.
            _currentTarget.Clear(); // Очищаем список текущих целей.

            // Проходимся по всем целям и добавляем их в очищенный список.
            foreach (var target in GetAllTargets())
            {
                _currentTarget.Add(target);
            }

            // Добавляем базу противника, если список целей пуст.
            if (_currentTarget.Count == 0)
            {
                _currentTarget.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            // Сортируем цели по расстоянию до базы.
            SortByDistanceToOwnBase(_currentTarget);

            // Рассчитываем номер текущего юнита и определяем цель для атаки.
            int currentUnitNumber = UnitNumber % _currentTarget.Count;
            
            if (_currentTarget.Count > currentUnitNumber && IsTargetInRange(_currentTarget[currentUnitNumber]))
            {
                result.Add(_currentTarget[currentUnitNumber]);
            }

            return result;
            ///////////////////////////////////////
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