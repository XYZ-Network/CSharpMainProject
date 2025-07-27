using Model.Config;
using Systems.BuffSystem;
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
        public bool IsDead { get; }
        public BaseUnitPath ActivePath { get; }
        public ModifiableParams Params { get; }
        
        // Just to demo how some conditions can be used in Buffs. In general we should have
        // some type system for units.
        public BaseUnitBrain Brain { get; }
    }
}