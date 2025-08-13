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

            float maxHeight = 0.6f * totalDistance;
            float t2 = t * 2f;
            float arcFactor = -((t2 - 1f) * (t2 - 1f)) + 1f;
            localHeight = maxHeight * arcFactor;

            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}