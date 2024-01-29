using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public abstract class BaseUnitPath
    {
        public Vector2Int StartPoint => Vector2Int.zero;
        public Vector2Int EndPoint => Vector2Int.zero;
    }
}