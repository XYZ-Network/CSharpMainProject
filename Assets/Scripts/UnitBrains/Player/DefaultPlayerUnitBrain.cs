using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.Scripts.UnitBrains.Pathfinding;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
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

        public override Vector2Int GetNextStep()
        {

            // 1. Получаем вражескую базу
            int enemyID = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyID];

            // 2. Получаем рекомендуемую цель 
            var recommendationTarget = coordinator.GetRecomendationTarget();

            var recommendationPosition = coordinator.GetRecommendedPosition();

            // Если цели нет, идем к вражеской базе
            if (recommendationTarget == null)
            {
                //Debug.Log("Нет рекомендуемой цели, иду к вражеской базе");
                _activePath = new AStar(runtimeModel, unit.Pos, enemyBase);
                return _activePath.GetNextStepFrom(unit.Pos);
            }

            // 3. Проверяем что у цели есть позиция
            if (recommendationTarget.Pos == null)
            {
                //Debug.Log("У цели нет позиции, иду к вражеской базе");
                _activePath = new AStar(runtimeModel, unit.Pos, enemyBase);
                return _activePath.GetNextStepFrom(unit.Pos);
            }

            
            float attackRange = unit.Config.AttackRange;
            float distanceToTarget = Vector2Int.Distance(unit.Pos, recommendationTarget.Pos);

            
            if (distanceToTarget > attackRange * 2) // Если цель дальше чем 2 радиуса атаки
            {
                
                List<Vector2Int> reachableTargets = SelectTargets();

                if (reachableTargets.Count != 0) // Если есть достижимые цели
                {
                    //Debug.Log("Есть достижимые цели, но цель далеко - иду к рекомендуемой цели");
                    _activePath = new AStar(runtimeModel, unit.Pos, recommendationTarget.Pos);
                    return _activePath.GetNextStepFrom(unit.Pos);
                }
                else // Если нет достижимых целей
                {
                    //Debug.Log("Нет достижимых целей, иду к вражеской базе");
                    _activePath = new AStar(runtimeModel, unit.Pos, enemyBase);
                    return _activePath.GetNextStepFrom(unit.Pos);
                }
            }
            else // Если цель в пределах 2х радиусов атаки
            {
                //Debug.Log("Цель близко, атакую или приближаюсь");
                _activePath = new AStar(runtimeModel, unit.Pos, recommendationTarget.Pos);
                return _activePath.GetNextStepFrom(unit.Pos);
            }
        }
        //protected override List<Vector2Int> SelectTargets()
        //{
        //    var result = SingltonCoordinator.GetInstance().GetRecomendationTarget();
        //    return result;
        //}
    }
}