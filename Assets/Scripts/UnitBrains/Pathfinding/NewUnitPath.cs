using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class NewUnitPath : BaseUnitPath
    {
        protected Vector2Int[] dx = {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };
        protected const int MaxLength = 150;

        public NewUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint) : base(runtimeModel, startPoint, endPoint)
        {

        }

        protected override void Calculate()
        {
            var listPath = FindPath();
            // Проверка на случай, если ничего не было записано в найденный путь
            if (FindPath().Count < 1)
            {
                path = null;
            }
            else
            {
                path = listPath.ToArray();
            }
        }

        protected List<Vector2Int> FindPath()
        {
            // Проверка на случай, если старт и цель совпадают или находятся в 1 клетке друг от друга
            bool isCardinalDirection = dx.Contains(endPoint - startPoint);
            if (startPoint.Equals(endPoint) || isCardinalDirection == true)
                return new List<Vector2Int> { startPoint };

            Node startNode = new Node(startPoint);
            Node targetNode = new Node(endPoint);

            List<Node> openNode = new List<Node> { startNode };
            HashSet<Node> closedList = new HashSet<Node>();
            int counter = 0;
            Node bestNodeSoFar = startNode; // Хранит лучший найденный узел (ближайший к цели)
            float bestDistanceSoFar = float.MaxValue; // Расстояние лучшего узла до цели

            // Инициализируем стартовый узел
            startNode.CalculateEstimate(targetNode.Pos);
            startNode.CalculateValue();

            while (openNode.Count > 0 && counter < MaxLength)
            {
                int minIndex = 0;
                Node currentNode = openNode[0];

                for (int i = 1; i < openNode.Count; i++)
                {
                    if (openNode[i].Value < currentNode.Value)
                    {
                        currentNode = openNode[i];
                        minIndex = i;
                    }
                }

                // Удаляем по индексу
                openNode.RemoveAt(minIndex);

                // Пропускаем, если узел уже обработан
                if (closedList.Contains(currentNode))
                    continue;

                closedList.Add(currentNode);

                // Обновляем лучший узел, если текущий ближе к цели
                float currentDistance = CalculateDistance(currentNode.Pos, endPoint);
                if (currentDistance < bestDistanceSoFar)
                {
                    bestDistanceSoFar = currentDistance;
                    bestNodeSoFar = currentNode;
                }

                // Проверяем, достигли ли цели
                if (endPoint.Equals(currentNode.Pos))
                    return buildPath(currentNode);

                // Исследуем соседей
                foreach (var direction in dx)
                {
                    var nextStep = direction + currentNode.Pos;

                    // Проверяем проходимость
                    if (!runtimeModel.IsTileWalkable(nextStep) && !nextStep.Equals(endPoint))
                        continue;

                    Node neighbor = new Node(nextStep);

                    // Пропускаем, если узел уже закрыт
                    if (closedList.Contains(neighbor))
                        continue;

                    // Проверяем, есть ли сосед уже в openNode
                    bool alreadyInOpen = false;
                    for (int i = 0; i < openNode.Count; i++)
                    {
                        if (openNode[i].Equals(neighbor))
                        {
                            // Если нашли узел с той же позицией, обновляем его параметры,
                            // если новый путь лучше (меньшее Value)
                            int newCost = currentNode.Cost + neighbor.Cost;
                            if (newCost < openNode[i].Cost)
                            {
                                openNode[i].Parent = currentNode;
                                openNode[i].Cost = newCost;
                                openNode[i].CalculateValue();
                            }
                            alreadyInOpen = true;
                            break;
                        }
                    }

                    if (!alreadyInOpen)
                    {
                        neighbor.Parent = currentNode;
                        neighbor.CalculateEstimate(targetNode.Pos);
                        neighbor.CalculateValue();
                        openNode.Add(neighbor);
                    }
                }

                counter++;
            }

            // Если достигли MaxLength, но не нашли путь — возвращаем путь к ближайшему найденному узлу
            return buildPath(bestNodeSoFar);
        }

        private float CalculateDistance(Vector2Int a, Vector2Int b)
        {
            var diff = a - b;
            return Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
        }

        protected List<Vector2Int> buildPath(Node node)
        {
            List<Vector2Int> result = new();
            while (node != null)
            {
                result.Add(node.Pos);
                node = node.Parent;
            }
            result.Reverse();
            return result;
        }
    }
}
