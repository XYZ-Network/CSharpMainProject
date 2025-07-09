using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace UnitBrains.Pathfinding
{
    class GridGraph : IGraph<Vector2Int>
    {
        private IReadOnlyRuntimeModel _runtimeModel;
        private Vector2Int _targetPos;
        private bool _isPlayerUnit;
        
        private static readonly int[,] Directions = new int[4, 2] {
            { 0, 1 },
            { 0, -1 },
            { 1, 0 },
            { -1, 0 },
        };

        public GridGraph(IReadOnlyRuntimeModel runtimeModel, Vector2Int targetPos, bool isPlayerUnit)
        {
            _runtimeModel = runtimeModel;
            _targetPos = targetPos;
            _isPlayerUnit = isPlayerUnit;
        }
        
        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int node)
        {
            List<Vector2Int> neighbors = new();

            for (int i = 0; i < Directions.GetLength(0); i++) {
                Vector2Int neighbor = new Vector2Int(
                    node.x + Directions[i, 0],
                    node.y + Directions[i, 1]
                );

                IEnumerable<IReadOnlyUnit> opposingUnits = _isPlayerUnit
                    ? _runtimeModel.RoBotUnits
                    : _runtimeModel.RoPlayerUnits;
                
                // Important. When we calculate path through enemy unit - we must consider them also as transparent,
                // i.e. as walkable tile. Otherwise, when we have a lot of opposing units that surround their base,
                // and unit is targeted at base, it will fail to find path. In any case unit stops and shoots
                // on approaching to another unit. 
                bool isOpposingUnits = opposingUnits.Any((unit) => unit.Pos == neighbor);
                
                // Also make an exception for target point, as when it's base, it should also be considered as walkable
                // to properly find path.
                if (neighbor == _targetPos || isOpposingUnits || _runtimeModel.IsTileWalkable(neighbor)) {
                    neighbors.Add(neighbor);
                }
            }
    
            return neighbors;            
        }

        public float GetEdgeCost(Vector2Int node, Vector2Int neighbor)
        {
            return 1.0f;
        }

        public float GetHeuristic(Vector2Int node, Vector2Int targetNode)
        {
            return Math.Abs(node.x - targetNode.x) + Math.Abs(node.y - targetNode.y);
        }
    }
    
    public class AStarUnitPath : BaseUnitPath
    {
        private bool _isPlayerUnit;

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