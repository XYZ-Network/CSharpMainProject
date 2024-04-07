using System.Collections.Generic;
using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyMap
    {
        public bool this[int x, int y] { get; }
        public bool this[Vector2Int pos] { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Vector2Int> Bases { get; }
    }
}