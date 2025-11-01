using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.ReadOnly;
using Utilities;

namespace Utilities
{
    public class BuffDebuffSystem
    {
        private readonly Dictionary<IReadOnlyUnit, List<BuffDebuff>> _buffs = new();
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
            if (!_buffs.ContainsKey(unit))
            {
                _buffs[unit] = new List<BuffDebuff>();
            }
            _buffs[unit].Add(buff);
        }

        public float GetModifiedMoveSpeedDelay(IReadOnlyUnit unit)
        {
            float totalModifier = 1f;
            if (_buffs.ContainsKey(unit))
            {
                foreach (var buff in _buffs[unit])
                {
                    if (buff is MoveBuffDebuff moveBuff)
                    {
                        totalModifier *= moveBuff.MoveSpeedModifier;
                    }
                }
            }

            return unit.Config.MoveDelay / totalModifier;
        }

        public float GetModifiedAttackSpeedDelay(IReadOnlyUnit unit)
        {
            float totalModifier = 1f;
            if (_buffs.ContainsKey(unit))
            {
                foreach (var buff in _buffs[unit])
                {
                    if (buff is AttackBuffDebuff attackBuff)
                    {
                        totalModifier *= attackBuff.AttackSpeedModifier;
                    }
                }
            }

            return unit.Config.AttackDelay / totalModifier;
        }

        public bool HasBuffs(IReadOnlyUnit unit)
        {
            return _buffs.ContainsKey(unit) && _buffs[unit].Count > 0;
        }

        private void Update(float deltaTime)
        {
            foreach (var unitBuffs in _buffs.Values)
            {
                for (int i = unitBuffs.Count - 1; i >= 0; i--)
                {
                    var buff = unitBuffs[i];
                    buff.Duration -= deltaTime;
                    if (buff.Duration <= 0)
                    {
                        unitBuffs.RemoveAt(i);
                    }
                }
            }
        }
    }

    public abstract class BuffDebuff
    {
        public float Duration { get; set; }

        public BuffDebuff(float duration)
        {
            Duration = duration;
        }
    }

    public class MoveBuffDebuff : BuffDebuff
    {
        public float MoveSpeedModifier { get; }

        public MoveBuffDebuff(float duration, float moveSpeedModifier) : base(duration)
        {
            MoveSpeedModifier = moveSpeedModifier;
        }
    }

    public class AttackBuffDebuff : BuffDebuff
    {
        public float AttackSpeedModifier { get; }
        public AttackBuffDebuff(float duration, float attackSpeedModifier) : base(duration)
        {
            AttackSpeedModifier = attackSpeedModifier;
        }
    }
}