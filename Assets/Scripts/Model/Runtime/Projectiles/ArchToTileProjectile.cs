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
            float timeDelta = timeSinceStart / _timeToTarget;
            
            Pos = Vector2.Lerp(StartPoint, _target, timeDelta);
            
            float totalDistance = _totalDistance;
            
            float maxHeight = totalDistance * 0.6f;

            int pow = 2;
            float localHeight = maxHeight * (-(float)Math.Pow(timeDelta * 2 - 1, pow) + 1);
            
            Height = localHeight;
            if (time > StartTime + _timeToTarget) {
                Hit(_target);
            }
        }
    }
}