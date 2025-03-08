using System.Collections.Generic;
using Model.Runtime;
using UnityEngine;

namespace UnitBrains.BuffSystem
{
    public class ShootFasterBuff : Buff<Unit>
    {
        public override string Name { get; }
        private int _shootFasterIndex = 4;
        
        protected List<Unit> _buffsToRemove = new List<Unit>();
        
        public ShootFasterBuff(float duration) : base(duration)
        {
            Name = $"{this.GetType().Name}";
        }
        
        public override void ApplyBuff(Unit unit)
        {
            if(CanApply(unit))
            {
                unit.AttackDelay /= _shootFasterIndex;
                Debug.Log($"Buff '{Name}' Add to unit '{unit.Config.Name}'.");
                _targetUnitList.Add(unit);//
            }
        }

        public override void RemoveBuff()
        {
            foreach (var unit in _targetUnitList)
            {
                unit.AttackDelay *= _shootFasterIndex;
                Debug.Log($"Buff {Name} Remove from unit '{unit.Config.Name}'.");
                _buffsToRemove.Add(unit);
            }

            foreach (var b in _buffsToRemove) _targetUnitList.Remove(b);
        }

        public override bool CanApply(Unit unit) 
            //=> unit.Brain != null && unit.Brain.GetType() == typeof(DefaultPlayerUnitBrain);
            => unit.Brain != null && unit.Config.Name == "Cobra 2";
    }
}
