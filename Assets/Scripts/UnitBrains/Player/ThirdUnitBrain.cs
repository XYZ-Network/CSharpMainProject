using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        // Настройки стрельбы очередями
        private const int BurstSize = 3;         // Количество снарядов в очереди
        private const float ShotInterval = 0.1f; // Интервал между выстрелами
        private const float BurstCooldown = 2f;  // Перезарядка между очередями

        private int _shotsRemaining;             // Оставшиеся выстрелы в очереди
        private float _nextShotTime;             // Время следующего выстрела
        private float _nextBurstTime;            // Время следующей очереди
        private Vector2Int _currentTarget;       // Текущая цель

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            // Если очередь завершена и перезарядка тоже - сбрасываем таймеры
            if (_shotsRemaining == 0 && time >= _nextBurstTime)
            {
                _nextBurstTime = time + BurstCooldown;
            }
        }

        /*protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            Vector2 direction = ((Vector2)(forTarget - unit.Pos)).normalized;

            for (int i = 0; i < BurstSize; i++)
            {
                // Смещение каждого последующего снаряда
                Vector2 offset = direction * i * 0.1f;

                var projectile = CreateProjectile(forTarget);
                projectile.AddStartShift(offset);
                AddProjectileToList(projectile, intoList);
            }

            // Перезарядка после очереди
            _nextBurstTime = Time.time + BurstCooldown;
        }*/


        private void StartNewBurst(Vector2Int target)
        {
            _currentTarget = target;
            _shotsRemaining = BurstSize;
            _nextShotTime = Time.time;
            _nextBurstTime = Time.time + BurstCooldown + (BurstSize * ShotInterval);
        }

        /*private void FireShot(List<BaseProjectile> intoList)
        {
            // Создаем снаряд со смещением для эффекта очереди
            Vector2 direction = ((Vector2)(_currentTarget - unit.Pos)).normalized;
            Vector2 offset = direction * (BurstSize - _shotsRemaining) * 0.1f;

            var projectile = CreateProjectile(_currentTarget);
            projectile.AddStartShift(offset);
            AddProjectileToList(projectile, intoList);

            _shotsRemaining--;
            _nextShotTime = Time.time + ShotInterval;
        }*/

        protected override List<Vector2Int> SelectTargets()
        {
            var result = base.SelectTargets();

            // Если есть активная очередь - продолжаем стрелять по текущей цели
            if (_shotsRemaining > 0 && IsTargetInRange(_currentTarget))
            {
                return new List<Vector2Int> { _currentTarget };
            }

            // Иначе выбираем новую цель
            _shotsRemaining = 0; // Прерываем текущую очередь

            // Оставляем только одну цель
            while (result.Count > 1)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }
    }
}