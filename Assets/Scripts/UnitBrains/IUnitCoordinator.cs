using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains
{
    public interface IUnitCoordinator
    {
        public Vector2Int RecommendedTarget { get; }
        public Vector2Int RecommendedPosition { get; }
    }
}