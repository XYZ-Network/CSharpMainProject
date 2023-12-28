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
            var timeSinceStart = time - StartTime;
            var t = timeSinceStart / _timeToTarget;
            
            Pos = Vector2.Lerp(StartPoint, _target, t);
            
            Height = 0f;
            var totalDistance = _totalDistance;
            ///////////////////////////////////////
            // Insert you code here
            ///////////////////////////////////////
            
            
            ///////////////////////////////////////
            // End of the code to insert
            ///////////////////////////////////////
            
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}