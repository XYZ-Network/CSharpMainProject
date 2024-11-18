using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

#nullable enable

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        private const int MoveCost = 10;

        private int MaxLength
        {
            get
            {
                if (runtimeModel == null)
                {
                    Debug.LogWarning("RuntimeModel is not initialized!");
                    return 0; // Или любое значение по умолчанию
                }

                if (runtimeModel.RoMap == null)
                {
                    Debug.LogWarning("RoMap is not initialized!");
                    return 0; // Или любое значение по умолчанию
                }

                return runtimeModel.RoMap.Width * runtimeModel.RoMap.Height;
            }
        }

        public AStarUnitPath(int x, int y, IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        protected override void Calculate()
        {
            var currentPoint = startPoint;
            var result = new List<Vector2Int> { startPoint };
            var visited = new HashSet<Vector2Int> { startPoint };
            var counter = 0;

            while (currentPoint != endPoint && counter++ < MaxLength)
            {
                var currentNode = new Nodes(currentPoint.x, currentPoint.y);
                currentNode.CalculateEstimate(endPoint.x, endPoint.y);
                currentNode.SetCost(10);

                var nextPoint = CalcNextStepTowards(currentPoint, endPoint);

                if (!visited.Contains(nextPoint) && runtimeModel.IsTileWalkable(nextPoint))
                {
                    result.Add(nextPoint);
                    visited.Add(nextPoint);
                    currentPoint = nextPoint;
                }
                else
                {
                    if (!TryStep(currentPoint, visited, result))
                    {
                        Debug.LogWarning($"No available steps from {currentPoint}. Stopping calculation.");
                        break;
                    }
                }
            }

            path = result.ToArray();
            Debug.LogWarning($"Calculating path from {startPoint} to {endPoint}");
        }

        private static readonly Vector2Int[] possibleSteps = {
             new Vector2Int(1, 0),
             new Vector2Int(-1, 0),
             new Vector2Int(0, 1),
             new Vector2Int(0, -1)
        };

        private bool TryStep(Vector2Int currentPoint, HashSet<Vector2Int> visited, List<Vector2Int> result)
        {
            foreach (var step in possibleSteps)
            {
                Vector2Int newStep = new Vector2Int(currentPoint.x + step.x, currentPoint.y + step.y);
                if (!visited.Contains(newStep) && runtimeModel.IsTileWalkable(newStep))
                {
                    result.Add(newStep);
                    visited.Add(newStep);
                    return true;
                }
            }

            return false;
        }

        private bool IsValid(Vector2Int cell)
        {
            bool isValidX = cell.x >= 0 && cell.x < runtimeModel.RoMap.Width;
            bool isValidY = cell.y >= 0 && cell.y < runtimeModel.RoMap.Height;

            return isValidX && isValidY && runtimeModel.IsTileWalkable(cell);
        }

        private Vector2Int CalcNextStepTowards(Vector2Int current, Vector2Int target)
        {
            Vector2Int bestStep = current;
            float bestDistance = Vector2Int.Distance(current, target);

            foreach (var step in possibleSteps)
            {
                Vector2Int potentialStep = current + step;
                if (IsValid(potentialStep))
                {
                    float distance = Vector2Int.Distance(potentialStep, target);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestStep = potentialStep;
                    }
                }
            }

            return bestStep;
        }
        public void ExecutePathfinding()
        {
            Calculate(); // Вызов внутреннего метода
            Debug.Log("Pathfinding executed.");
        }
    }
}