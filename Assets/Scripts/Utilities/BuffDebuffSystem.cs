using System;
using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Utilities
{
    public class BuffDebuffSystem
    {
        private readonly Dictionary<IBuffableUnit, List<BuffDebuff>> _buffs = new();
        private readonly TimeUtil _timeUtil;

        public BuffDebuffSystem()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddUpdateAction(Update);
        }

        ~BuffDebuffSystem()
        {
            _timeUtil.RemoveUpdateAction(Update);
        }

        public void ApplyBuff(IReadOnlyUnit unit, BuffDebuff buff)
        {
            if (unit is not IBuffableUnit buffableUnit)
            {
                Debug.LogWarning($"Attempted to apply {buff.GetType().Name} to non buffable unit {unit?.GetType().Name}");
                return;
            }

            if (unit.Config.Name == "Magic Damn")
            {
                Debug.LogWarning($"Attempted to apply {buff.GetType().Name} to {unit.Config.Name}, but this unit cannot be buffed.");
                return;
            }

            if (!buff.CanApply(unit))
            {
                return;
            }

            if (!_buffs.TryGetValue(buffableUnit, out var unitBuffs))
            {
                unitBuffs = new List<BuffDebuff>();
                _buffs[buffableUnit] = unitBuffs;
            }

            buff.ApplyTo(buffableUnit);
            unitBuffs.Add(buff);
        }

        public bool HasBuffs(IReadOnlyUnit unit)
        {
            return unit is IBuffableUnit buffable &&
                   _buffs.TryGetValue(buffable, out var buffs) &&
                   buffs.Count > 0;
        }

        private void Update(float deltaTime)
        {
            foreach (var kvp in _buffs)
            {
                var buffable = kvp.Key;
                var unitBuffs = kvp.Value;

                for (int i = unitBuffs.Count - 1; i >= 0; i--)
                {
                    var buff = unitBuffs[i];
                    buff.Tick(deltaTime);
                    if (buff.IsExpired)
                    {
                        buff.RevertFrom(buffable);
                        unitBuffs.RemoveAt(i);
                    }
                }
            }
        }
    }

    public abstract class BuffDebuff
    {
        public float Duration { get; private set; }
        public bool IsExpired => Duration <= 0f;

        protected BuffDebuff(float duration)
        {
            Duration = duration;
        }

        public void Tick(float deltaTime)
        {
            Duration -= deltaTime;
        }

        public abstract bool CanApply(IReadOnlyUnit unit);
        internal abstract void ApplyTo(IBuffableUnit unit);
        internal abstract void RevertFrom(IBuffableUnit unit);
    }

    public abstract class BuffDebuff<TUnit> : BuffDebuff where TUnit : class, IBuffableUnit
    {
        protected BuffDebuff(float duration) : base(duration)
        {
        }

        public override bool CanApply(IReadOnlyUnit unit)
        {
            if (unit is not TUnit typed)
            {
                return false;
            }

            return CanApplyTo(typed);
        }

        protected virtual bool CanApplyTo(TUnit unit) => true;

        protected abstract void ApplyEffect(TUnit unit);
        protected abstract void RevertEffect(TUnit unit);

        internal override void ApplyTo(IBuffableUnit unit)
        {
            if (unit is not TUnit typed)
                throw new InvalidOperationException(
                    $"Cannot use {GetType().Name} on {unit?.GetType().Name}");

            ApplyEffect(typed);
        }

        internal override void RevertFrom(IBuffableUnit unit)
        {
            if (unit is not TUnit typed)
                throw new InvalidOperationException(
                    $"Cannot revert {GetType().Name} from {unit?.GetType().Name}");

            RevertEffect(typed);
        }
    }

    public class MoveBuffDebuff : BuffDebuff<IBuffableUnit>
    {
        private readonly float _moveSpeedModifier;

        public MoveBuffDebuff(float duration, float moveSpeedModifier) : base(duration)
        {
            _moveSpeedModifier = moveSpeedModifier;
        }

        protected override void ApplyEffect(IBuffableUnit unit) => unit.ApplyMoveSpeedMultiplier(_moveSpeedModifier);

        protected override void RevertEffect(IBuffableUnit unit) => unit.ApplyMoveSpeedMultiplier(1f / _moveSpeedModifier);
    }

    public class AttackBuffDebuff : BuffDebuff<IBuffableUnit>
    {
        private readonly float _attackSpeedModifier;
        public AttackBuffDebuff(float duration, float attackSpeedModifier) : base(duration)
        {
            _attackSpeedModifier = attackSpeedModifier;
        }

        protected override void ApplyEffect(IBuffableUnit unit) => unit.ApplyAttackSpeedMultiplier(_attackSpeedModifier);

        protected override void RevertEffect(IBuffableUnit unit) => unit.ApplyAttackSpeedMultiplier(1f / _attackSpeedModifier);
    }

    public class DoubleShotBuff : BuffDebuff<IBuffableUnit>
    {
        private readonly int _extraShots;
        private readonly string _targetUnitName;

        public DoubleShotBuff(float duration, int extraShots, string targetUnitName = "Cobra Commando")
            : base(duration)
        {
            _extraShots = extraShots;
            _targetUnitName = targetUnitName;
        }

        protected override bool CanApplyTo(IBuffableUnit unit) =>
            string.Equals(unit.Config?.Name, _targetUnitName, StringComparison.Ordinal);

        protected override void ApplyEffect(IBuffableUnit unit) => unit.ModifyExtraAttackExecutions(_extraShots);

        protected override void RevertEffect(IBuffableUnit unit) => unit.ModifyExtraAttackExecutions(-_extraShots);
    }

    public class AttackRangeBuff : BuffDebuff<IBuffableUnit>
    {
        private readonly float _rangeMultiplier;
        private readonly string _targetUnitName;

        public AttackRangeBuff(float duration, float rangeMultiplier, string targetUnitName = "Ironclad Behemoth")
            : base(duration)
        {
            _rangeMultiplier = rangeMultiplier;
            _targetUnitName = targetUnitName;
        }

        protected override bool CanApplyTo(IBuffableUnit unit) =>
            string.Equals(unit.Config?.Name, _targetUnitName, StringComparison.Ordinal);

        protected override void ApplyEffect(IBuffableUnit unit) => unit.ApplyAttackRangeMultiplier(_rangeMultiplier);

        protected override void RevertEffect(IBuffableUnit unit) => unit.ApplyAttackRangeMultiplier(1f / _rangeMultiplier);
    }
}
