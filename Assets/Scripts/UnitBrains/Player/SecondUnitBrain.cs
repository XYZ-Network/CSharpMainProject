using System;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace UnitBrains.Player
{
    // Мозг второго юнита: использует рекомендацию координатора как цель движения,
    // но добавляет небольшой уникальный оффсет, чтобы снизить толпление.
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => string.Empty;

        public override Vector2Int GetNextStep()
        {
            // Если есть непосредственные цели в радиусе — остаёмся и стреляем
            if (HasTargetsInRange())
                return unit.Pos;

            var coordPoint = PlayerUnitsCoordinatorSingleton.Instance.RecommendedPoint;

            // создаём небольшой детерминированный оффсет, зависящий от позиции юнита,
            // чтобы юниты распределялись вокруг рекомендованной точки
            int offsetX = (unit.Pos.x % 3) - 1; // -1,0,1
            int offsetY = (unit.Pos.y % 3) - 1;

            var target = coordPoint + new Vector2Int(offsetX, offsetY);

            // используем A* локально (не пишем в приватный _activePath базового класса),
            // чтобы получить следующий шаг к целевой позиции
            var path = new AStarUnitPath(runtimeModel, unit.Pos, target);
            return path.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // По умолчанию — используем логику базового мозга,
            // но можно расширить поведение под SecondUnit (в будущем).
            return base.SelectTargets();
        }
    }
}