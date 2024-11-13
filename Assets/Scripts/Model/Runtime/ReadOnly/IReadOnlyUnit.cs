using Assets.Scripts.UnitBrains.Buffs;
using Model.Config;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public UnitConfigModifiers Modifiers { get; }
        public Vector2Int Pos { get; }
        public int Health { get; }
        public BaseUnitPath ActivePath { get; }
        public BaseUnitBrain UnitBrain { get; }
    }
}