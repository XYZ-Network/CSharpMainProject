using Model.Runtime;
using UnitBrains;
using UnityEngine;

namespace Assets.Scripts.UnitBrains
{
    public abstract class Buff<T> where T : BaseUnitBrain
    {
        public float Duration { get; private set; }
        public float MoveModifier { get; private set; }
        public float AttackModifier { get; private set; }
        public virtual bool IsBuff => (MoveModifier >= 1f && AttackModifier >= 1f);
        public Buff(float duration, float moveModifier, float attackModifier)
        {
            Duration = duration;
            MoveModifier = moveModifier;
            AttackModifier = attackModifier;
        }
        public void ReduceDuration()
        {
            Duration -= 0.1f;
        }
        public virtual bool CanBeAppliedTo(Unit unit) => unit.Brain is not null;
        public virtual void ApplyEffect(Unit unit)
        {
            if (MoveModifier != 1f)
                unit.ChangeMoveSpeed(1f / MoveModifier);
            if (AttackModifier != 1f)
                unit.ChangeAttackSpeed(1f / AttackModifier);
        }
        public virtual void RemoveEffect(Unit unit)
        {
            unit.ResetAttackSpeed();
            unit.ResetMoveSpeed();
        }
    }
}
