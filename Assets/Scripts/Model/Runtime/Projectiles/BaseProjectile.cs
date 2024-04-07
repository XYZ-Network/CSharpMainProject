using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model.Runtime.Projectiles
{
    public abstract class BaseProjectile : IReadOnlyProjectile
    {
        public static BaseProjectile Create(ProjectileType type, Unit unit, Vector2Int startPoint, Vector2Int target, int damage)
        {
            switch (type)
            {
                case ProjectileType.ArchToTile:
                    return new ArchToTileProjectile(unit, target, damage, startPoint);
                case ProjectileType.HorizontalLine:
                    return new HorizontalLineProjectile(unit, target, damage, startPoint);
                case ProjectileType.Homing:
                    return new HomingProjectile(unit, target, damage, startPoint);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public Vector2 Position => Pos + _startShift;
        public float Height { get; protected set; }
        public int Damage { get; }
        public bool HadHit { get; private set; }

        public Vector2Int HitTile { get; private set; }
        protected Vector2 StartPoint { get; private set; }
        protected float StartTime { get; private set; } = float.MinValue;
        protected Vector2 Pos { get; set; }
        private Vector2 _startShift;
        
        protected abstract void UpdateImpl(float deltaTime, float time);

        protected BaseProjectile(int damage, Vector2Int startPoint)
        {
            Damage = damage;
            StartPoint = startPoint;
        }

        public void AddStartShift(Vector2 shift)
        {
            _startShift = shift;
        }

        public void Update(float deltaTime, float time)
        {
            if (HadHit)
                return;

            if (StartTime < 0f)
                StartTime = time;
            
            UpdateImpl(deltaTime, time);
        }

        protected void Hit(Vector2Int tile)
        {
            HadHit = true;
            HitTile = tile;
        }
    }
}