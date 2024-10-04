using Model.Config;
using UnitBrains;
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

        public void setAttackModifier(float modifier = 1);

        public void setMoveModifier(float modifier = 1);

        public BaseUnitBrain getBrain();
    }
}