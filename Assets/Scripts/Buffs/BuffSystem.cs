using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Buffs
{
    public sealed class BuffSystem
    {
        private readonly Dictionary<IReadOnlyUnit, List<IUnitBuff>> _buffs = new();

        public BuffSystem(TimeUtil timeUtil)
        {
        }

        public void AddBuff(IReadOnlyUnit unit, IUnitBuff buff)
        {
            if (unit == null || buff == null)
                return;

            if (!buff.CanApplyTo(unit))
                return;

            if (!_buffs.ContainsKey(unit))
                _buffs[unit] = new List<IUnitBuff>();

            _buffs[unit].Add(buff);
            ReapplyAllBuffs();
        }

        public bool HasAnyBuff(IReadOnlyUnit unit)
        {
            if (unit == null)
                return false;

            return _buffs.TryGetValue(unit, out var buffs) && buffs.Count > 0;
        }

        public void Update(float deltaTime)
        {
            var units = _buffs.Keys.ToList();

            foreach (var unit in units)
            {
                if (!_buffs.TryGetValue(unit, out var buffs))
                    continue;

                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    buffs[i].Tick(deltaTime);

                    if (buffs[i].IsFinished)
                        buffs.RemoveAt(i);
                }

                if (buffs.Count == 0)
                    _buffs.Remove(unit);
            }

            ReapplyAllBuffs();
        }

        public void Clear()
        {
            _buffs.Clear();
            ReapplyAllBuffs();
        }

        private void ReapplyAllBuffs()
        {
            var runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            foreach (var unit in runtimeModel.RoUnits.OfType<Unit>())
                unit.ResetBuffState();

            foreach (var pair in _buffs)
            {
                foreach (var buff in pair.Value)
                    buff.ApplyTo(pair.Key);
            }
        }
    }
}