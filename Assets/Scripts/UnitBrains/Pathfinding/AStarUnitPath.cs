using Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    internal partial class AStarUnitPath : BaseUnitPath
    {
        private const int MaxCellsToCheck = 1000;
        private const int NormalStepCost = 10; // Стоимость обычного шага
        private const int PenaltyStepCost = 14; // Штраф за занятые клетки

        private static Vector2Int[] Directions = {
            new(1, 0),
            new(-1, 0), 
            new(0, 1), 
            new(0, -1),
        };

        public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate() => CalculatePath();

        private void CalculatePath()
        {
            var startNode = new PathNode(startPoint, 0);
            var targetNode = new PathNode(endPoint, 0);

            var openList = new List<PathNode> { startNode }; // клетки, которые предстоит проверить
            var closedList = new HashSet<PathNode>(); // проверенные клетки

            int checkedCells = 0; // Счётчик обработанных клеток
            PathNode current = null;
            bool found = false; // флаг

            while (openList.Count > 0)
            {
                current = openList[0];
                for (int i = 1; i < openList.Count; i++) // Находим узел с минимальной общей стоимостью (Value = CostFromStart + Estimate)
                {
                    if (openList[i].Value < current.Value)
                        current = openList[i];
                }
                openList.Remove(current);
                closedList.Add(current);


                if (current.Position == targetNode.Position) // Если достигли цели — заканчиваем поиск
                {
                    found = true;
                    break;
                }

                foreach (var dir in Directions) // Перебираем все соседние клетки
                {
                    checkedCells++;

                    var neighborPos = current.Position + dir;

                    if (!runtimeModel.IsTileWalkable(neighborPos)
                        && neighborPos != targetNode.Position
                        && !IsUnitAtPos(neighborPos))
                        continue; // Пропускаем клетки, которые полностью непроходимы

                    if (closedList.Contains(new PathNode(neighborPos, 0)))
                        continue; // Пропускаем уже проверенные клетки

                    int stepCost = (!runtimeModel.IsTileWalkable(neighborPos) && IsUnitAtPos(neighborPos)) ? PenaltyStepCost : NormalStepCost;
                    int totalCost = current.CostFromStart + stepCost;
                    var existingOpen = openList.FirstOrDefault(node => node.Position == neighborPos);

                    if (existingOpen == null)
                    {
                        var neighborNode = new PathNode(neighborPos, totalCost)
                        {
                            Parent = current
                        };
                        neighborNode.CalculateEstimate(targetNode.Position);
                        neighborNode.CalculateValue();
                        openList.Add(neighborNode);
                    }

                    else if (totalCost < existingOpen.CostFromStart)
                    {
                        existingOpen.CostFromStart = totalCost;
                        existingOpen.Parent = current;
                        existingOpen.CalculateValue();
                    }

                    if (checkedCells >= MaxCellsToCheck)
                        break;
                }

                if (checkedCells >= MaxCellsToCheck)
                    break;
            }

            path = found ? ReconstructPath(current) : new Vector2Int[] { startPoint };

            if (found)
            {
                int baseCost = 0;
                int penaltyCost = 0;

                for (int i = 1; i < path.Length; i++)
                {
                    Vector2Int to = path[i];
                    bool isPenalty = !runtimeModel.IsTileWalkable(to) && IsUnitAtPos(to);
                    baseCost += NormalStepCost;

                    if (isPenalty)
                    {
                        penaltyCost += (PenaltyStepCost - NormalStepCost);
                    }
                }

                int totalPathCost = baseCost + penaltyCost;

                //Debug.Log($"[A*] Путь найден после проверки {checkedCells} клеток от {startPoint} до {endPoint}." +
                //          $" Стоимость пути: базовая — {baseCost}, штрафы — {penaltyCost}, всего — {totalPathCost}.");
            }
            else
            {
                //Debug.Log($"[A*] Путь не найден после проверки {checkedCells} клеток от {startPoint} до {endPoint}.");
            }
        }

        private bool IsUnitAtPos(Vector2Int pos) =>
            runtimeModel.RoUnits.Any(u => u.Pos == pos);

        private Vector2Int[] ReconstructPath(PathNode endNode)
        {
            var result = new List<Vector2Int>();
            for (var node = endNode; node != null; node = node.Parent)
                result.Add(node.Position);
            result.Reverse();
            return result.ToArray();
        }
    }
}