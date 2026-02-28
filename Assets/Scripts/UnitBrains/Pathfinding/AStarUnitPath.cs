using Model;
using System;
using System.Collections.Generic;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using System.Linq;

public class AStarUnitPath : BaseUnitPath
{
    // Направления движения: влево, вверх, вправо, вниз
    private Vector2Int[] dxy = {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1)
    };

    public AStarUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        : base(runtimeModel, startPoint, endPoint)
    {
    }

    protected override void Calculate()
    {
        List<Vector2Int> result = FindPath();
        path = result.ToArray();
    }

    private List<Vector2Int> FindPath()
    {
        // Нода старта и цели
        PathNode startNode = new PathNode(startPoint);
        PathNode targetNode = new PathNode(endPoint);

        // Инициализируем стартовую ноду
        startNode.GCost = 0; // g(n) = 0 для старта
        startNode.CalculateHCost(endPoint); // Вычисляем h(n), для конечной
        startNode.CalculateFCost(); // f(n) = g(n) + h(n)

        // OpenList содержит ноды которе потом будут обобатываеться
        List<PathNode> openList = new List<PathNode> { startNode };

        // ClosedSet содержит уже обработанные 
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        while (openList.Count > 0)
        {
            // Находим ноду с наименьшим f(n)
            PathNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                // Выбор оптимальной ноды
                if (openList[i].FCost < currentNode.FCost ||
                    (openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost))
                {
                    currentNode = openList[i];
                }
            }

            // Удаляем из openList и добавляем в closedSet
            openList.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            // Если достигли цели - восстанавливаем путь
            if (currentNode.Position == endPoint)
            {
                return ReconstructPath(currentNode);
            }

            // Проверяем всех соседей
            foreach (var direction in dxy)
            {
                Vector2Int neighborPos = currentNode.Position + direction;

                // Пропускаем уже обработанные позиции
                if (closedSet.Contains(neighborPos))
                    continue;

                // Проверяем возможность пройти
                bool isWalkable = runtimeModel.IsTileWalkable(neighborPos);
                bool isTarget = neighborPos == endPoint;
                bool hasUnit = IsUnitAtPos(neighborPos);

                if (!isWalkable && !isTarget && !hasUnit)
                    continue;

                // Вычисляем стоимость движения на эту клетку
                int moveCost = isWalkable ? 10 : 30;
                // Новая g(n) = g(n) текущей ноды + стоимость движения
                int newGCost = currentNode.GCost + moveCost;

                // Ищем, есть ли эта позиция уже в openList
                PathNode existingNode = openList.FirstOrDefault(n => n.Position == neighborPos);

                if (existingNode == null)
                {
                    // Создаем новую ноду
                    PathNode newNode = new PathNode(neighborPos);
                    newNode.GCost = newGCost; // Сумма стоимость от старта
                    newNode.CalculateHCost(endPoint);
                    newNode.CalculateFCost(); 
                    newNode.Parent = currentNode; // Запоминаем откуда пришли
                    openList.Add(newNode);
                }
                else if (newGCost < existingNode.GCost)
                {
                    // Если нашли более короткий путь - обновляем
                    existingNode.GCost = newGCost;
                    existingNode.CalculateFCost();
                    existingNode.Parent = currentNode;
                }
            }
        }

        // если не нашли путь, то возвращаем стартовую позицию
        return new List<Vector2Int> { startPoint };
    }

    // Восстанавливаем путь от цели к старту
    private List<Vector2Int> ReconstructPath(PathNode endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        PathNode currentNode = endNode;

        // Идем от цели к старту по Parent
        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        // Реверс для того чтобы путь шел от старта к цели
        path.Reverse();
        return path;
    }

    // Проверяем, есть ли юнит на позиции
    private bool IsUnitAtPos(Vector2Int pos) =>
        runtimeModel.RoUnits.Any(u => u.Pos == pos);
}

public class PathNode
{
    private IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>();

    public Vector2Int Position;
    public int GCost;
    public int HCost;
    public int FCost;
    public PathNode Parent;

    public PathNode(Vector2Int position)
    {
        Position = position;
        GCost = 0; // Будет установлено при добавлении в openList
    }

    // Вычисляем h(n) - расстояние до цели
    public void CalculateHCost(Vector2Int target)
    {
        HCost = Math.Abs(Position.x - target.x) + Math.Abs(Position.y - target.y);
    }

    // Вычисляем f(n) = g(n) + h(n)
    public void CalculateFCost()
    {
        FCost = GCost + HCost;
    }

    // Переопределяем Equals для корректной работы с Contains и HashSet
    public override bool Equals(object obj)
    {
        if (obj is not PathNode other)
            return false;

        return Position.x == other.Position.x && Position.y == other.Position.y;
    }

    // Переопределяем GetHashCode для работы с HashSet
    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}
