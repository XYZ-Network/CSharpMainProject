using System.Collections.Generic;
using Model;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public abstract class BaseUnitPath
    {
        // Свойство для получения начальной точки пути (только для чтения)
        public Vector2Int StartPoint => startPoint;
        // Свойство для получения конечной точки пути (только для чтения)
        public Vector2Int EndPoint => endPoint;

        // Защищенные поля для хранения данных
        protected readonly IReadOnlyRuntimeModel runtimeModel; // Модель времени выполнения для доступа к карте и юнитам
        protected readonly Vector2Int startPoint; // Начальная точка пути
        protected readonly Vector2Int endPoint; // Конечная точка пути
        protected Vector2Int[] path = null; // Массив точек пути (null, если путь еще не вычислен)

        // Абстрактный метод для вычисления пути
        // Должен быть реализован в наследниках (AStarUnitPath, DummyUnitPath и т.д.)
        protected abstract void Calculate();

        // Метод для получения всех точек пути
        // Если путь еще не вычислен, вызывает Calculate()
        public IEnumerable<Vector2Int> GetPath()
        {
            // Проверяем, был ли уже вычислен путь
            if (path == null)
            {
                // Если нет, вычисляем его
                Calculate();
            }

            // Возвращаем вычисленный путь
            return path;
        }

        // Метод для получения следующего шага от указанной позиции юнита
        // Используется для пошагового движения по пути
        public Vector2Int GetNextStepFrom(Vector2Int unitPos)
        {
            // Флаг для отслеживания, нашли ли мы текущую позицию юнита в пути
            bool found = false;

            // Проходим по всем точкам пути
            foreach (Vector2Int cell in GetPath())
            {
                // Если мы уже нашли текущую позицию, возвращаем следующую точку
                if (found)
                {
                    return cell;
                }

                // Проверяем, является ли текущая точка пути позицией юнита
                if (cell == unitPos)
                {
                    found = true;
                }
            }

            // Если позиция юнита не найдена в пути (ошибка)
            Debug.LogError($"Unit at position {unitPos} is not on the calculated path from {startPoint} to {endPoint}");

            // Возвращаем текущую позицию как запасной вариант
            return unitPos;
        }

        // Конструктор базового класса
        // Принимает модель времени выполнения, начальную и конечную точки
        protected BaseUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
        {
            // Проверяем, что модель не null
            if (runtimeModel == null)
            {
                Debug.LogError("RuntimeModel cannot be null in BaseUnitPath constructor");
            }

            // Сохраняем переданные параметры
            this.runtimeModel = runtimeModel;
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            // Путь изначально не вычислен
            this.path = null;
        }

        // Вспомогательный метод для проверки, вычислен ли уже путь
        protected bool IsPathCalculated()
        {
            return path != null;
        }

        // Вспомогательный метод для получения длины пути
        public int GetPathLength()
        {
            // Получаем путь (вычисляем, если нужно)
            Vector2Int[] calculatedPath = GetPath() as Vector2Int[];

            // Возвращаем длину пути или 0, если путь не существует
            return calculatedPath?.Length ?? 0;
        }

        // Вспомогательный метод для проверки, содержит ли путь указанную точку
        public bool ContainsPoint(Vector2Int point)
        {
            // Получаем путь
            IEnumerable<Vector2Int> pathPoints = GetPath();

            // Проверяем каждую точку пути
            foreach (Vector2Int pathPoint in pathPoints)
            {
                if (pathPoint == point)
                {
                    return true;
                }
            }

            return false;
        }

        // Переопределение ToString() для удобной отладки
        public override string ToString()
        {
            if (path == null)
            {
                return $"Path from {startPoint} to {endPoint} [NOT CALCULATED]";
            }
            else
            {
                return $"Path from {startPoint} to {endPoint} [{path.Length} steps]";
            }
        }
    }
}