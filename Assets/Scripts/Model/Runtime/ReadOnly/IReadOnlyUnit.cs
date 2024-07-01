using Model.Config;
using System;
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
        int PlayerId { get; }
        bool HasBuff(Type buffType);
    }
}