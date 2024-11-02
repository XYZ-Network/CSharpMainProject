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

            float maxheight = totalDistance * 0.60f;
            /*/ указал максимальную высоту по формуле которая на сайте float b = a * 0,34f 
             * вот другая если не подойдет
             * float maxHeight = (totalDistance * 100)*60
             /*/

             localHeight = maxheight * (-(t * 2 - 1) * (t * 2 - 1) + 1);
            // я надеюсь формулу не надо расписывать и не надо ставить f в конце, потому что выдает ошибку. 

           





            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}