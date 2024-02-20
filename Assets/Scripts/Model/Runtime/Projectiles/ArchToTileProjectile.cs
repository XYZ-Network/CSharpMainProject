using ActionGameFramework.Helpers;
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
            // Insert you code here
            ///////////////////////////////////////
            
            float maxHeight = 0.6f * totalDistance; //60% от дистанции
            float partBallisticsFormula = t * 2 - 1; //Вынес повторяющиеся части формулы баллистики t * 2 - 1

            localHeight = maxHeight * (-partBallisticsFormula * partBallisticsFormula + 1); //Расчет формулы баллистики

            ///////////////////////////////////////
            // End of the code to insert
            ///////////////////////////////////////
            
            Height = localHeight;
            if (time > StartTime + _timeToTarget)
                Hit(_target);
        }
    }
}