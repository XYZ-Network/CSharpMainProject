using Codice.Client.Common.TreeGrouper;
using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace UnitBrains.Pathfinding
{
    public class SmartUnitPath : BaseUnitPath
    {
        // Смещения для перемещения по координатам
        private int[] dx = { -1, 0, 1, 0 }; // Изменение по оси X
        private int[] dy = { 0, 1, 0, -1 }; // Изменение по оси Y
        private int MaxLength = 600; // Максимальная длина пути

        
        public SmartUnitPath(IReadOnlyRuntimeModel runtimeModel, Vector2Int startPoint, Vector2Int endPoint)
            : base(runtimeModel, startPoint, endPoint)
        {
        }

        // Реализация абстрактного метода для расчета пути
        protected override void Calculate()
        {
            var points = FindPath(); // Поиск пути
            // Если путь не найден, возвращаем начальную точку как путь
            path = points == null ? new Vector2Int[] { StartPoint, StartPoint } : points.Select((point) => new Vector2Int(point.X, point.Y)).ToArray();
        }

        // Метод для поиска пути от начальной точки к конечной
        public List<Point> FindPath()
        {
            Point startPoint = new Point(this.startPoint.x, this.startPoint.y); 
            Point endPoint = new Point(this.endPoint.x, this.endPoint.y); 

            List<Point> openList = new List<Point>() { startPoint }; 
            List<Point> closedList = new List<Point>(); 

            
            while (openList.Count > 0)
            {
                
                Point currentPoint = openList[0];
                foreach (var point in openList)
                {
                    if (point.Value < currentPoint.Value)
                        currentPoint = point; 
                }

                openList.Remove(currentPoint); 
                closedList.Add(currentPoint); 

               
                if (currentPoint.X == endPoint.X && currentPoint.Y == endPoint.Y || closedList.Count > MaxLength)
                {
                    List<Point> path = new List<Point>();
                    while (currentPoint != null) 
                    {
                        path.Add(currentPoint);
                        currentPoint = currentPoint.Parent;
                    }
                    path.Reverse(); 
                    return path; 
                }

              
                for (int i = 0; i < dx.Length; i++)
                {
                    int newX = currentPoint.X + dx[i]; // Новая координата X
                    int newY = currentPoint.Y + dy[i]; // Новая координата Y

                    // Проверяем, действительна ли новая точка
                    if (IsValid(new Vector2Int(newX, newY)))
                    {
                        Point nextPoint = new Point(newX, newY); 

                        
                        if (closedList.Contains(nextPoint))
                            continue;

                        nextPoint.Parent = currentPoint; // Устанавливаем родительскую точку
                        nextPoint.CalculateEstimate(endPoint.X, endPoint.Y); // Вычисляем оценку
                        nextPoint.CalculateValue(); // Вычисляем стоимость

                        openList.Add(nextPoint); // Добавляем точку в открытый список
                    }
                }
            }

            return null; 
        }

        // Метод для проверки, является ли точка действительной
        private bool IsValid(Vector2Int point)
        {
            bool isValidX = point.y >= 0 && point.y < runtimeModel.RoMap.Height; // Проверка по  Y
            bool isValidY = point.x >= 0 && point.x < runtimeModel.RoMap.Width; // Проверка по X
            bool isBase = point.x == endPoint.x && point.y == endPoint.y; // Проверка на конечную точку
            return isValidX && isValidY && (runtimeModel.IsTileWalkable(point) || isBase); // Возвращаем true, если точка действительна
        }
    }
}
