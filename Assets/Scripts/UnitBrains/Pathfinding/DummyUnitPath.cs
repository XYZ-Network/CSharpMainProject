using System.Collections.Generic;
using Model;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class DummyUnitPath : BaseUnitPath
    {
        private const int MaxLength = 100; // Максимальная длина пути

       
        public DummyUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        // Реализация абстрактного метода для расчета пути
        protected override void Calculate()
        {
            var currentPoint = startPoint; // Начальная точка
            var result = new List<Vector2Int> { startPoint }; // Список для хранения найденного пути
            var counter = 0; // Счетчик, чтобы избежать бесконечного цикла

            
            while (currentPoint != endPoint && counter++ < MaxLength)
            {
                var nextStep = CalcNextStepTowards(currentPoint, endPoint); 
                var hasLoop = result.Contains(nextStep); 
                result.Add(nextStep); // Добавление следующего шага в путь

                
                if (hasLoop)
                    break;

                currentPoint = nextStep; 
            }

            path = result.ToArray(); 
        }

        // Вычисление следующего шага к целевой позиции
        private Vector2Int CalcNextStepTowards(Vector2Int fromPos, Vector2Int toPos)
        {
            var diff = toPos - fromPos;
            var stepDiff = diff.SignOrZero(); 
            var nextStep = fromPos + stepDiff; 

            // Проверка доступности клетки
            if (runtimeModel.IsTileWalkable(nextStep))
                return nextStep;

            // Проверка на длинный шаг
            if (stepDiff.sqrMagnitude > 1)
            {
                var partStep0 = fromPos + new Vector2Int(stepDiff.x, 0); // Шаг по оси X
                if (runtimeModel.IsTileWalkable(partStep0))
                    return partStep0;

                var partStep1 = fromPos + new Vector2Int(0, stepDiff.y); // Шаг по оси Y
                if (runtimeModel.IsTileWalkable(partStep1))
                    return partStep1;
            }

            // Проверка побочных шагов слева
            var sideStep0 = fromPos + new Vector2Int(stepDiff.y, -stepDiff.x); // Поворот влево
            var shiftedStep0 = fromPos + (sideStep0 + stepDiff).SignOrZero(); // Сдвиг влево
            if (runtimeModel.IsTileWalkable(shiftedStep0))
                return shiftedStep0;

            // Проверка побочных шагов справа
            var sideStep1 = fromPos + new Vector2Int(-stepDiff.y, stepDiff.x); // Поворот вправо
            var shiftedStep1 = fromPos + (sideStep1 + stepDiff).SignOrZero(); // Сдвиг вправо
            if (runtimeModel.IsTileWalkable(shiftedStep1))
                return shiftedStep1;

            // Проверка доступности боковых шагов
            if (runtimeModel.IsTileWalkable(sideStep0))
                return sideStep0;

            if (runtimeModel.IsTileWalkable(sideStep1))
                return sideStep1;

            // Если ни один шаг не доступен, остаемся на месте
            return fromPos;
        }
    }
}