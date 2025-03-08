using System.Collections.Generic;
using Model.Runtime;
using UnitBrains.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UnitBrains.BuffSystem
{
    public class MoveFasterBuff : Buff<Unit>
    {
        private int _moveFasterIndex = 2;
        public override string Name { get; }
        
        public MoveFasterBuff(float duration) : base(duration)
        {
            Name = $"{this.GetType().Name}";
        }
        
        public override void ApplyBuff(Unit unit)
        {
            if (CanApply(unit))
            {
                unit.MoveDelay /= _moveFasterIndex;
                Debug.Log($"Buff '{Name}' Add to unit '{unit.Config.Name}'.");
                _targetUnitList.Add(unit);
            }
        }

        public override void RemoveBuff()
        {
            foreach (var unit in _targetUnitList)
            {
                unit.MoveDelay *= _moveFasterIndex;
                Debug.Log($"Buff {Name} Remove from unit '{unit.Config.Name}'.");
                _buffsToRemove.Add(unit);
            }

            foreach (var b in _buffsToRemove) _targetUnitList.Remove(b);
        }

        public override bool CanApply(Unit unit) 
            //=> unit.Brain != null && unit.Brain.GetType() == typeof(SecondUnitBrain);
            => unit.Brain != null && unit.Config.Name == "Ironclad Behemoth";
    }
}