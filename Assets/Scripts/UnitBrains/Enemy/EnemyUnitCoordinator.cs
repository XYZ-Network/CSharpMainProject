using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnitBrains.Enemy
{
    public class EnemyUnitCoordinator : IUnitCoordinator
    {
        public Vector2Int RecommendedTarget { get; private set; }
        public Vector2Int RecommendedPosition { get; private set; }

        public EnemyUnitCoordinator() { }
    }
}