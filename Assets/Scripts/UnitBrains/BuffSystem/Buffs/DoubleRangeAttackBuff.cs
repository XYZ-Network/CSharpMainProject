using System.Collections.Generic;
using Model.Runtime;
using UnitBrains.Player;
using UnityEngine;

namespace UnitBrains.BuffSystem
{
    public class DoubleRangeAttackBuff : Buff<Unit>
    {
        private int _attackRangeIndex = 2;
        
        public DoubleRangeAttackBuff(float duration) : base(duration)
        {
            Name = $"{this.GetType().Name}";
        }

        public override string Name { get; }
        public override void ApplyBuff(Unit unit)
        {
            if(CanApply(unit))
            {
                unit.Brain.AttackRange *= _attackRangeIndex;
                Debug.Log($"Buff '{Name}' Add to unit '{unit.Config.Name}'.");
                _targetUnitList.Add(unit);
            }
        }

        public override void RemoveBuff()
        {
            foreach (var unit in _targetUnitList)
            {
                unit.Brain.AttackRange /= _attackRangeIndex;
                Debug.Log($"Buff {Name} Remove from unit '{unit.Config.Name}'.");
                _buffsToRemove.Add(unit);
            }

            foreach (var b in _buffsToRemove) _targetUnitList.Remove(b);
        }

        public override bool CanApply(Unit unit)
            //=> unit.Brain != null && unit.Brain.GetType() == typeof(BaseUnitBrain);
            => unit.Brain != null && unit.Config.Name == "Cobra Commando";
    }
}