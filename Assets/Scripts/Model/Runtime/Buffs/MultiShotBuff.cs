using System;
using Model.Runtime.ReadOnly;

namespace Model.Runtime.Buffs
{
    public class MultiShotBuff : IBuff
    {
        public static bool CanCastTo(IReadOnlyUnit unit)
            => unit.Config.Name.Equals("Cobra Commando");
        
        public IReadOnlyUnit Unit { get; }
        private float _remainingTime;
        private float _value;

        public MultiShotBuff (IReadOnlyUnit unit, float remainingTime, float value)
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
            Unit.ModifyProjectilesPerShot(1);

        public void Remove() => 
            Unit.ResetProjectilesPerShot();
    }
}