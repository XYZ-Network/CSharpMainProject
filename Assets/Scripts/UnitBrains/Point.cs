using System;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    
    public class Point
    {
        
        public int X { get; private set; }
        public int Y { get; private set; }

        // Стоимость перемещения к этой точке
        public int Cost { get; private set; } = 10;

        // Оценка и общая стоимость для алгоритма A*
        public int Estimate { get; private set; }
        public int Value { get; private set; }

        // Родительская точка для восстановления пути
        public Point Parent { get; set; }

        // Конструктор для инициализации координат
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        
        public void CalculateEstimate(int targetX, int targetY)
        {
            Estimate = Math.Abs(X - targetX) + Math.Abs(Y - targetY); 
        }

        
        public void CalculateValue()
        {
            Value = Cost + Estimate; // Общее значение
        }

        // Установить сравнение двух точек
        public override bool Equals(object? obj)
        {
            if (obj is not Point point)
                return false;

            return X == point.X && Y == point.Y; // Сравнение по координатам
        }

        // Генерация уникального хэш-кода на основе координат
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y); // Уникальный хэш-код
        }
    }
}