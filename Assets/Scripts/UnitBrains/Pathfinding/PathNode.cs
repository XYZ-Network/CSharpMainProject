using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Pathfinding
{
    public class PathNode
    {
        public Vector2Int Position { get; set; }
        public PathNode Parent { get; set; }
        public int StartCost { get; set; } // Стоимость от старта
        public int HashCost { get; set; } // Эвристическая стоимость до цели
        public int FullCost => StartCost + HashCost; // Общая стоимость

        public PathNode(Vector2Int position)
        {
            Position = position;
        }
    }
}
