using UnityEngine;

namespace Model.Runtime.Projectiles
{
    public class HorizontalLineProjectile : BaseProjectile
    {
        public HorizontalLineProjectile(Unit unit, Vector2Int target, int damage, Vector2Int startPoint) : base(damage, startPoint)
        {
        }
        
        protected override void UpdateImpl(float deltaTime, float time)
        {
        }
    }
}