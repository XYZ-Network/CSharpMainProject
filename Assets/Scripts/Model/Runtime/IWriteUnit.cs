using Model.Config;
using Model.Runtime.ReadOnly;
using System.Collections;
using UnitBrains.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Model.Runtime
{
    public interface IWriteUnit
    {
        public float AttackDelay { get; set; }
        public int AttackCount { get; set; }
        public float MoveDelay { get; set; }
        public float AttackRange { get; set; }

        public void ResetAttackDelayToDefault();
        public void ResetMoveDelayToDefault();
        public void ResetAttackRangeToDefault();
        public void ResetAttackCountToDefault();
    }
}