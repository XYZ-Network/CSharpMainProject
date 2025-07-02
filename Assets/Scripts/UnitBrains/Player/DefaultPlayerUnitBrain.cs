using Model;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private const float AttackRangeMultiplier = 2f;

        protected float DistanceToOwnBase(Vector2Int fromPos) =>
            Vector2Int.Distance(fromPos, runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);

        protected void SortByDistanceToOwnBase(List<Vector2Int> list)
        {
            list.Sort(CompareByDistanceToOwnBase);
        }

        private int CompareByDistanceToOwnBase(Vector2Int a, Vector2Int b)
        {
            var distanceA = DistanceToOwnBase(a);
            var distanceB = DistanceToOwnBase(b);
            return distanceA.CompareTo(distanceB);
        }

        public override Vector2Int GetNextStep()
        {
            var coord = Coordinator;
            var target = coord.RecommendedTarget;
            var center = coord.RecommendedPoint;

            bool isBaseTarget = runtimeModel.RoMap.Bases.Contains(target);
            Vector2Int point = isBaseTarget
                    ? target
                    : GetDistributedPointAround(center, unit.Pos);

            float distToTarget = Vector2Int.Distance(unit.Pos, target);
            bool canAttack = distToTarget <= coord.StandardAttackRange * AttackRangeMultiplier;

            Vector2Int destination = canAttack ? target : point;

            if (destination == unit.Pos)
                return unit.Pos;

            var path = new AStarUnitPath(runtimeModel, unit.Pos, destination);


            var fullPath = path.GetPath().ToList();

            bool hasCurrentCell = fullPath.Contains(unit.Pos);

            if (!hasCurrentCell || fullPath.Count < 2)
            {
                var dir = destination - unit.Pos;
                dir.x = Mathf.Clamp(dir.x, -1, 1);
                dir.y = Mathf.Clamp(dir.y, -1, 1);
                return unit.Pos + dir;
            }

            VisualizePath(path);
            return path.GetNextStepFrom(unit.Pos);
        }

        private static readonly Vector2Int[] DistributionOffsets =
        {
            new(0, 0),
            new(1, 0),  new(-1, 0),
            new(0, 1),  new(0, -1),
            new(1, 1),  new(-1, -1),
            new(1, -1), new(-1, 1)
        };

        protected Vector2Int GetDistributedPointAround(Vector2Int center, Vector2Int unitPos)
        {
            int hash = Mathf.Abs(unitPos.x ^ unitPos.y);
            int idx = hash % DistributionOffsets.Length;
            return center + DistributionOffsets[idx];
        }
    }
}