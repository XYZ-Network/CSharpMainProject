using Model.Config;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; }
        public int Health { get; }
        public BaseUnitPath ActivePath { get; }
        
        public void ModifyMoveSpeed(float value);
        public void ResetMoveSpeed();

        public void ModifyAttackSpeed(float value);
        public void ResetAttackSpeed();
        
        public void ModifyAttackRange(int value);
        public void ResetAttackRange();
        
        public void ModifyProjectilesPerShot(int value);
        public void ResetProjectilesPerShot();

        public void SetInvulnerability(bool value);
    }
}