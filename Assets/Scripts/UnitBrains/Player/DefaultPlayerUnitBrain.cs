using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private const int MaxTargetsCount = 3;
        private static int _counter = 0;
        private int _unitIndex = -1;

        protected Vector2Int GetAssignedTarget()
        {
            if (_unitIndex == -1)
            {
                _unitIndex = _counter;
                _counter++;
            }
            List<Vector2Int> targets = new List<Vector2Int>();
            IEnumerable<Vector2Int> allTargets = GetAllTargets();//Список всех целей

            foreach (var target in allTargets) // Проходимся по списку всех целей
            {
                targets.Add(target);
            }

            if (targets.Count == 0)
            {
                var idEnemy = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId; // определяем чей юнит
                Vector2Int basePosition = runtimeModel.RoMap.Bases[idEnemy]; // записываем позицию вражеской базы
                targets.Add(basePosition); // добавляем в список целей к которым нужно двигаться вражескую базу
            }

            SortByDistanceToOwnBase(targets); //сортируем список целей по возврастанию дистанции
            int targetsCount = Mathf.Min(targets.Count, MaxTargetsCount); // определяем на какое кол. целей делим наших юнитов
            int index = _unitIndex % targetsCount; //выбирем индекс цели для юнита

            return targets[index];
        }

        protected Vector2Int GetStepTowardsAssignedTarget()
        {
            Vector2Int target = GetAssignedTarget();
            if (IsTargetInRange(target))
            {
                return unit.Pos;
            }
            return unit.Pos.CalcNextStepTowards(target);
        }
        protected List<Vector2Int> GetAssignedTargetIfInRange()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            
            if (IsTargetInRange(GetAssignedTarget()))
            {
                result.Add(GetAssignedTarget());
            }
            return result; //Возвращаем цель
        }
        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }
        
        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }
    }
}