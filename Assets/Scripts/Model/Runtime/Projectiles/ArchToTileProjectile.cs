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

            // 1. Вычисляем максимальную высоту (60% от общей дистанции)
            float maxHeight = totalDistance * 0.6f;

            // 2. Выносим повторяющуюся часть формулы в отдельную переменную
            float tFactor = t * 2f - 1f;

            // 3. Считаем итоговую высоту, используя возведение в квадрат
            localHeight = maxHeight * (-(tFactor * tFactor) + 1f);
            
            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}
