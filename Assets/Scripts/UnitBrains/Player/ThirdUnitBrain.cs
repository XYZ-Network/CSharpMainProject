using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

namespace UnitBrains.Player
{
    public enum UnitState
    {
        Moving,
        Shooting
    }
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _timer;
        private float switchTime = 0.1f;
        private UnitState unitState = UnitState.Moving;
        private bool mode;
        public float AttackRangeModifier = 1f;
        public override Vector2Int GetNextStep()
        {
            
            Vector2Int nextPos = base.GetNextStep();
            {
                if (nextPos == unit.Pos)
                {
                    if (unitState != UnitState.Shooting)
                    {
                        mode = true;
                    }
                    unitState = UnitState.Shooting;
                }
                else 
                {
                    if (unitState != UnitState.Moving)
                    {
                        mode = true;
                    }
                    unitState = UnitState.Moving;
                }
            }
            return mode ? unit.Pos : nextPos;
        }
        protected override List<Vector2Int> SelectTargets()
        {
            if (mode)
            {
                return new List<Vector2Int>();
            }
            if (unitState == UnitState.Shooting)
            {
                return base.SelectTargets();
            }
            return new List<Vector2Int>();
        }
        public override void Update(float deltatime, float time)
        {
            if (mode)
            {
                _timer += Time.deltaTime;
                if (_timer >= switchTime)
                {
                    _timer = 0f;
                    mode = false;
                }
            }
            

        }
        protected override bool HasTargetsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange * AttackRangeModifier;
            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos;
                if (diff.sqrMagnitude < attackRangeSqr)
                    return true;
            }

            return false;
        }

        protected override bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange * AttackRangeModifier;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr;
        }
    
    }
}
