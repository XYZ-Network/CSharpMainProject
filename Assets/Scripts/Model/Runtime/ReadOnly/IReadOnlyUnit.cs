using Model.Config;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; } // позиция юнита
        public int Health { get; } // здоровье юнита
        public BaseUnitPath ActivePath { get; }
    }
}