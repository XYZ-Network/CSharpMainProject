using Model.Runtime;
using System;

namespace Assets.Scripts.Model.Runtime
{
    public interface IBuff
    {
        Unit Unit { get; }
        float Duration { get; set; }
        bool CanApply(Unit unit);
        void Apply(Unit unit);
        void Remove(Unit unit);
    }

    public abstract class Buff<T> : IBuff where T : Unit
    {
        protected Buff(float duration)
        {
            if (duration <= 0f)
                throw new ArgumentException("Продолжительность должна быть больше чем 0", nameof(duration));

            Duration = duration;
        }

        public T TypedUnit { get; private set; }
        public Unit Unit => TypedUnit;
        public float Duration { get; set; }

        public abstract bool CanApply(T unit);
        public abstract void Apply(T unit);
        public abstract void Remove(T unit);

        bool IBuff.CanApply(Unit unit)
        {
            return unit is T typedUnit && CanApply(typedUnit);
        }

        void IBuff.Apply(Unit unit)
        {
            if (unit is T typedUnit && CanApply(typedUnit))
            {
                TypedUnit = typedUnit;
                Apply(typedUnit);
            }
        }

        void IBuff.Remove(Unit unit)
        {
            if (unit is T typedUnit && TypedUnit == typedUnit)
            {
                Remove(typedUnit);
                TypedUnit = null;
            }
        }
    }
    
    public abstract class NumericModifierBuff<T> : Buff<T> where T : Unit, IUnitModifiable
    {
        protected readonly float _modifier;
        private readonly Action<T, float> _applyAction;
        private readonly Action<T, float> _removeAction;
        
        protected NumericModifierBuff(float duration, float modifier, Action<T, float> applyAction, Action<T, float> removeAction) : base(duration)
        {
            ValidateModifier(modifier);
            _modifier = modifier;
            _applyAction = applyAction;
            _removeAction = removeAction;
        }

        protected virtual void ValidateModifier(float modifier)
        {
            if (modifier <= 0f)
                throw new ArgumentException("Модификатор должен быть больше 0", nameof(modifier));
        }

        public override bool CanApply(T unit) => true;

        public override void Apply(T unit)
        {
            _applyAction?.Invoke(unit, _modifier);
        }

        public override void Remove(T unit)
        {
            _removeAction?.Invoke(unit, 1f / _modifier);
        }
    }

    public abstract class ToggleBuff<T> : Buff<T> where T : Unit, IUnitModifiable
    {
        private readonly Action<T> _enableAction;
        private readonly Action<T> _disableAction;

        protected ToggleBuff(float duration, Action<T> enableAction, Action<T> disableAction) : base(duration)
        {
            _enableAction = enableAction;
            _disableAction = disableAction;
        }

        public override bool CanApply(T unit) => true;

        public override void Apply(T unit)
        {
            _enableAction?.Invoke(unit);
        }

        public override void Remove(T unit)
        {
            _disableAction?.Invoke(unit);
        }
    }

    public class MoveSpeedBuff<T> : NumericModifierBuff<T> where T : Unit, IUnitModifiable
    {
        public MoveSpeedBuff(float duration, float modifier) : base(duration, modifier, (u, m) => u.ModifyMoveSpeed(m), (u, m) => u.ModifyMoveSpeed(m))
        {
        }
    }

    public class AttackSpeedBuff<T> : NumericModifierBuff<T> where T : Unit, IUnitModifiable
    {
        public AttackSpeedBuff(float duration, float modifier) : base(duration, modifier, (u, m) => u.ModifyAttackSpeed(m), (u, m) => u.ModifyAttackSpeed(m))
        {
        }
    }

    public class DoubleShotBuff<T> : ToggleBuff<T> where T : Unit, IUnitModifiable
    {
        public DoubleShotBuff(float duration) : base(duration, u => u.EnableDoubleShot(), u => u.DisableDoubleShot())
        {
        }
    }

    public class IncreasedRangeBuff<T> : NumericModifierBuff<T> where T : Unit, IUnitModifiable
    {
        public IncreasedRangeBuff(float duration, float rangeModifier) : base(duration, rangeModifier, (u, m) => u.ModifyAttackRange(m), (u, m) => u.ModifyAttackRange(m))
        {
        }
    }

    public abstract class Debuff<T> : NumericModifierBuff<T> where T : Unit, IUnitModifiable
    {
        protected Debuff(float duration, float modifier, Action<T, float> applyAction, Action<T, float> removeAction) : base(duration, modifier, applyAction, removeAction)
        {
        }

        protected override void ValidateModifier(float modifier)
        {
            if (modifier <= 0f || modifier > 1f)
                throw new ArgumentException("Модификатор должен быть между 0 и 1", nameof(modifier));
        }
    }

    public class MoveSlowDebuff<T> : Debuff<T> where T : Unit, IUnitModifiable
    {
        public MoveSlowDebuff(float duration, float modifier) : base(duration, modifier, (u, m) => u.ModifyMoveSpeed(m), (u, m) => u.ModifyMoveSpeed(m))
        {
        }
    }

    public class AttackSlowDebuff<T> : Debuff<T> where T : Unit, IUnitModifiable
    {
        public AttackSlowDebuff(float duration, float modifier) : base(duration, modifier, (u, m) => u.ModifyAttackSpeed(m), (u, m) => u.ModifyAttackSpeed(m))
        {
        }
    }

    public class BuffCarBuff : Buff<Unit>
    {
        private readonly float _moveSpeedModifier;
        public BuffCarBuff(float duration, float moveSpeedModifier) : base(duration)
        {
            if (moveSpeedModifier <= 0f)
                throw new ArgumentException("Модификатор скорости должен быть больше 0", nameof(moveSpeedModifier));

            _moveSpeedModifier = moveSpeedModifier;
        }

        public override bool CanApply(Unit unit)
        {
            return unit is IUnitModifiable;
        }

        public override void Apply(Unit unit)
        {
            if (unit is IUnitModifiable modifiableUnit)
            {
                modifiableUnit.ModifyMoveSpeed(_moveSpeedModifier);
            }
        }

        public override void Remove(Unit unit)
        {
            if (unit is IUnitModifiable modifiableUnit)
            {
                modifiableUnit.ModifyMoveSpeed(1f / _moveSpeedModifier);
            }
        }
    }
}