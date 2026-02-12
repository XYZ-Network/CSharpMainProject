using System;
using UnityEngine;

namespace Model.Runtime.Projectiles
{
    public class ArchToTileProjectile : BaseProjectile
    {
        private const float ProjectileSpeed = 7f;
        private readonly Vector2Int _target;
        private readonly float _timeToTarget;
        private readonly float _totalDistance;

        public ArchToTileProjectile(Unit unit, Vector2Int target, int damage, Vector2Int startPoint) : base(damage, startPoint)
        {
            _target = target;
            _totalDistance = Vector2.Distance(StartPoint, _target);
            _timeToTarget = _totalDistance / ProjectileSpeed;
        }

        protected override void UpdateImpl(float deltaTime, float time)
        {
            float timeSinceStart = time - StartTime;
            float t = timeSinceStart / _timeToTarget;

            Pos = Vector2.Lerp(StartPoint, _target, t);
            
            float localHeight = 0f;
            float totalDistance = _totalDistance;

            ///////////////////////////////////////
            // Insert your code here
            ///////////////////////////////////////
            float maxHeight = totalDistance * 0.6f;
            localHeight = maxHeight * (-(t * 2 - 1) * (t * 2 - 1) + 1);
            ///////////////////////////////////////
            // End of the code to insert
            ///////////////////////////////////////

            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }

    public class ProjectileHeightVisualizer
    {
        // Вывод графика высоты снаряда
        public static void DrawHeightGraph(float maxHeight, int steps = 20)
        {
            Console.WriteLine("ASCII-график высоты снаряда\n");

            // Шаг по нормализованному времени t от 0 до 1
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                // Формула локальной высоты
                float localHeight = maxHeight * (-(t * 2 - 1) * (t * 2 - 1) + 1);

                // Количество символов '*' для графического отображения
                int stars = (int)Math.Round(localHeight);

                // Печатаем t и визуальное отображение высоты
                Console.Write($"t={t:0.00} | ");
                for (int s = 0; s < stars; s++)
                    Console.Write("*");
                Console.WriteLine($" ({localHeight:0.00})");
            }
        }
    }

    // Пример вызова:
    class Program
    {
        static void Main()
        {
            float maxHeight = 10f; // Можно изменить на любую высоту
            ProjectileHeightVisualizer.DrawHeightGraph(maxHeight);
        }
    }
}