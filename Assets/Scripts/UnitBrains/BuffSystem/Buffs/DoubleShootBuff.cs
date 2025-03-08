using System.Collections.Generic;
using Model.Runtime;
using UnitBrains.Player;
using UnityEngine;

namespace UnitBrains.BuffSystem
{
    public class DoubleShootBuff : Buff<Unit>
    {
        public override string Name { get; }
        private int _doubleShootIndex = 2;
        private int _defaultShootIndex = 1;
        
        protected List<Unit> _buffsToRemove = new List<Unit>();
        
        public DoubleShootBuff(float duration) : base(duration)
        {
            Name = $"{this.GetType().Name}";
        }
        
        public override void ApplyBuff(Unit unit)
        {
            if(CanApply(unit))
            {
                unit.Brain.DoubleShootIndex = _doubleShootIndex;
                Debug.Log($"Buff '{Name}' Add to unit '{unit.Config.Name}'.");
                _targetUnitList.Add(unit);
            }
        }

        public override void RemoveBuff()
        {
            foreach (var unit in _targetUnitList)
            {
                unit.Brain.DoubleShootIndex = _defaultShootIndex;
                Debug.Log($"Buff {Name} Remove from unit '{unit.Config.Name}'.");
                _buffsToRemove.Add(unit);
            }

            foreach (var b in _buffsToRemove) _targetUnitList.Remove(b);
        }

        public override bool CanApply(Unit unit) 
            //=> unit.Brain != null && unit.Brain.GetType() == typeof(DefaultPlayerUnitBrain);
            => unit.Brain != null && unit.Config.Name == "Sky Serpent";
        
    }
}