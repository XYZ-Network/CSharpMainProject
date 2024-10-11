using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public abstract class BaseUnitPath
    {
        // Свойства для получения начальной и конечной точки пути
        public Vector2Int StartPoint => startPoint;
        public Vector2Int EndPoint => endPoint;

        // Защищенные поля, для модели и точек
        protected readonly IReadOnlyRuntimeModel runtimeModel;
        protected readonly Vector2Int startPoint;
        protected readonly Vector2Int endPoint;
        protected Vector2Int[] path = null; 

        // Абстрактный метод для расчета пути
        protected abstract void Calculate();

        // Получение пути 
        public IEnumerable<Vector2Int> GetPath()
        {
            if (path == null)
                Calculate(); // Выполнение расчета, если путь отсутствует

            return path; // Возврат рассчитанного пути
        }

        // Получение следующего шага на пути от текущей позиции юнита
        public Vector2Int GetNextStepFrom(Vector2Int unitPos)
        {
            var found = false; 
            foreach (var cell in GetPath()) // Проходим по всем ячейкам пути
            {
                if (found)
                    return cell; // Возвращаем следующую ячейку, если предыдущая найдена

                found = cell == unitPos; // Проверяем, равна ли текущая ячейка позиции юнита
            }

            Debug.LogError($"Unit {unitPos} is not on the path"); 
            return unitPos; 
        }

        // Конструктор для инициализации пути с заданной моделью и точками
        protected BaseUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        {
            this.runtimeModel = runtimeModel; // Инициализация модели
            this.startPoint = startPoint; // обоз. начальную точку
            this.endPoint = endPoint; // обоз. конечную точку 
        }
    }
}
