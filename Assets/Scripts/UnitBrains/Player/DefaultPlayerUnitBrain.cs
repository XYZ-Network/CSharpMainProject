using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using Model.Runtime;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        private BaseUnitPath _activePath;
        private const float attackRangeFactor = 2f;

        public override Vector2Int GetNextStep()
        {
            var target = Coordinator.GetInstance().SuggestedMoveTarget();

            if (IsTargetInRange(target))
                return unit.Pos;

            _activePath = new AStarPath(runtimeModel, unit.Pos, target);

            return _activePath.GetNextStepFrom(unit.Pos);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();

            var targetForAttack = Coordinator.GetInstance().SuggestedAttackTarget();

            float attackRange = attackRangeFactor * unit.Config.AttackRange;
            var attackRangeSqr = attackRange * attackRange;
            var diff = targetForAttack - unit.Pos;
            if (diff.sqrMagnitude < attackRangeSqr)
            {
                result.Add(targetForAttack);
            }

            return result;
        }

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
    }
}