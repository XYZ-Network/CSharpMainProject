using Model.Config;
using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; }
        public int Health { get; }
    }
}