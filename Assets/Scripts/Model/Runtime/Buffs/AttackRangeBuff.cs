using System;
using Model.Runtime.ReadOnly;

namespace Model.Runtime.Buffs
{
    public class AttackRangeBuff : IBuff
    {
        public static bool CanCastTo(IReadOnlyUnit unit)
            => unit.Config.Name.Equals("Ironclad Behemoth");
        
        public IReadOnlyUnit Unit { get; }
        private float _remainingTime;
        private int _value;

        public AttackRangeBuff (IReadOnlyUnit unit, float remainingTime, int value)
        {
            if (!CanCastTo(unit)) 
                throw new ArgumentException();
            
            Unit = unit;
            _remainingTime = remainingTime;
            _value = value;
        }
        
        public void ReduceRemainingTime(float time) => 
            _remainingTime -= time;

        public bool IsExpired() =>
            _remainingTime < 0;

        public void Apply() => 
            Unit.ModifyAttackRange(_value);

        public void Remove() => 
            Unit.ResetAttackRange();
    }
}