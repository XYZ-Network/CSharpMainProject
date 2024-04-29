using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using Unit = Model.Runtime.Unit;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum UnitAction
        {
            None,
            Moving,
            Attacking
        }

        private UnitAction currentAction = UnitAction.None;
        private float lastActionTime;
        private float actionDelay = 1f;
        private List<Vector2Int> _currentTargets = new List<Vector2Int>();


        public void StartMove()
        {
            if (Time.time >= lastActionTime && currentAction != UnitAction.Moving)
            {
                currentAction = UnitAction.Moving;
                lastActionTime = Time.time + actionDelay;
            }
        }

        public void StartAttack()
        {
            if (Time.time >= lastActionTime && currentAction != UnitAction.Attacking)
            {
                currentAction = UnitAction.Attacking;
                lastActionTime = Time.time + actionDelay;
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition;

            if (currentAction == UnitAction.Moving)
            {
                if (_currentTargets.Count > 0)
                    targetPosition = _currentTargets[0];
                else
                    targetPosition = unit.Pos;

                if (IsTargetInRange(targetPosition))
                    return unit.Pos;
                else
                    return targetPosition;
            }
            else if (currentAction == UnitAction.Attacking)
            {
                return unit.Pos;
            }

            return Vector2Int.zero;
        }
    }
}
    




