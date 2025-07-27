using System.Collections.Generic;
using Model.Runtime.ReadOnly;

namespace Systems.BuffSystem
{
    public class UnitParamSet
    {
        public float MoveDelay = 1f;
        public float AttackDelay = 1f;
        public float AttackRange = 1f;
        public int ProjectilesCount = 1;

        public UnitParamSet Clone() => (UnitParamSet)MemberwiseClone();
    }

    public class ModifiableParams
    {
        public readonly UnitParamSet Base;
        public UnitParamSet Current { get; private set; }
        public IReadOnlyUnit Owner { get; }

        private readonly List<IBuffEffect> _buffs = new();

        public ModifiableParams(UnitParamSet baseParams, IReadOnlyUnit owner)
        {
            Base = baseParams;
            Current = baseParams.Clone();
            Owner = owner;
        }

        public void AddBuff(IBuffEffect effect)
        {
            if (!_buffs.Contains(effect))
            {
                _buffs.Add(effect);
                Recalculate();
            }
        }

        public void RemoveBuff(IBuffEffect effect)
        {
            if (_buffs.Remove(effect))
            {
                Recalculate();
            }
        }

        public void RemoveAllBuffs()
        {
            _buffs.Clear();
            Recalculate();
        }

        public void Recalculate()
        {
            Current = Base.Clone();
            foreach (var buff in _buffs)
            {
                buff.Apply(this);
            }
        }

        public IEnumerable<IBuffEffect> ActiveEffects => _buffs;
    }
}