using Buffs.Buffs;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Player;

namespace Buffs
{
    namespace Buffs
    {
        public abstract class Buff<TBrain> where TBrain : BaseUnitBrain
        {
            public float Duration { get; private set; }

            protected Buff(float duration)
            {
                Duration = duration;
            }

            // Метод для применения баффа к юниту
            public abstract void Apply(IReadOnlyUnit unit);

            // Метод для отмены действия баффа
            public abstract void Expire(IReadOnlyUnit unit);

            // Метод для проверки, можно ли применить бафф к данному юниту
            public virtual bool CanApply(TBrain brain)
            {
                if (brain.GetType() == typeof(BaseUnitBrain))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    public class MovementBuff<TBrain> : Buff<TBrain> where TBrain : BaseUnitBrain
    {
        public MovementBuff(float duration) : base(duration)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            unit.Config.IncMoveDelay(-0.1f);
        }

        public override void Expire(IReadOnlyUnit unit)
        {
            unit.Config.IncMoveDelay(0.1f);
        }

    }

    public class AttackSpeedBuff<TBrain> : Buff<TBrain> where TBrain : BaseUnitBrain
    {
        public AttackSpeedBuff(float duration) : base(duration)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            unit.Config.IncAttackDelay(-0.2f);
        }

        public override void Expire(IReadOnlyUnit unit)
        {
            unit.Config.IncAttackDelay(0.2f);
        }
    }

    public class DoubleShootBuff<TBrain> : Buff<TBrain> where TBrain : BaseUnitBrain
    {
        public DoubleShootBuff(float duration) : base(duration)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {
            var brain = unit.Config.Brain as SecondUnitBrain;
            brain.shootingCounter = 2;
            
        }

        public override void Expire(IReadOnlyUnit unit)
        {
            SecondUnitBrain brain = unit.Config.Brain as SecondUnitBrain;
            brain.shootingCounter = 1;
        }

        public override bool CanApply(TBrain brain)
        {
            if (brain.GetType() == typeof(SecondUnitBrain))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class RadiusBuff<TBrain> : Buff<TBrain> where TBrain : BaseUnitBrain
    {
        public RadiusBuff(float duration) : base(duration)
        {
        }

        public override void Apply(IReadOnlyUnit unit)
        {

        }

        public override void Expire(IReadOnlyUnit unit)
        {

        }

        public override bool CanApply(TBrain brain)
        {
            if (brain.GetType() == typeof(ThirdUnitBrain))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

