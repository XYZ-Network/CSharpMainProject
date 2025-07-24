using System.Collections.Generic;
using Model;
using UnitBrains.Pathfinding.Graphs;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    public class AStarUnitPath : BaseUnitPath
    {
        private readonly bool _isPlayerUnit;

        public AStarUnitPath(
            IReadOnlyRuntimeModel runtimeModel,
            Vector2Int startPoint,
            Vector2Int endPoint,
            bool isPlayerUnit
        ) : base(runtimeModel, startPoint, endPoint)
        {
            _isPlayerUnit = isPlayerUnit;
        }

        protected override void Calculate()
        {
            GridGraph graph = new GridGraph(runtimeModel, endPoint, _isPlayerUnit);
            List<Vector2Int> result = GraphUtils.FindPathAStar(graph, startPoint, endPoint);
            
            path = result.ToArray();
        }
    }
}