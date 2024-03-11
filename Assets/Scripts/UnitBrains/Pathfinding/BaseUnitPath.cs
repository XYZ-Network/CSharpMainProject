using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public abstract class BaseUnitPath
    {
        public Vector2Int StartPoint => startPoint; //стартовая точка
        public Vector2Int EndPoint => endPoint; //конечная точка

        public IEnumerable<Vector2Int> Nodes { get; internal set; }

        protected readonly IReadOnlyRuntimeModel runtimeModel; //проверяет, является ли клетка проходимой
        protected readonly Vector2Int startPoint;
        protected readonly Vector2Int endPoint;
        protected Vector2Int[] path = null; //путь - массив из клеток

        protected abstract void Calculate(); //обязателен для переопределния в дочерних классах, 
        
        public IEnumerable<Vector2Int> GetPath()
        {
            if (path == null)
                Calculate(); //вызывается для расчета пути 
            
            return path; //если путь просчитан - вызываем путь 
        }

        public Vector2Int GetNextStepFrom(Vector2Int unitPos) //возвращает следующую ячейку от текущей позиции, передается позиция юнита
        {
            var found = false;
            foreach (var cell in GetPath())
            {
                if (found)
                    return cell;

                found = cell == unitPos;
            }

            Debug.LogError($"Unit {unitPos} is not on the path");
            return unitPos;
        }

        protected BaseUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.runtimeModel = runtimeModel;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
    }
}