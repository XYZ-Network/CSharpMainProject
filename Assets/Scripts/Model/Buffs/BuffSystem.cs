using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using Utilities;

namespace Model.Buffs
{
    public class BuffSystem
    {
        private static BuffSystem _instance;
        public static BuffSystem Instance => _instance ??= new BuffSystem();

        private readonly Dictionary<IReadOnlyUnit, List<Buff>> _buffs = new();
        private bool _subscribed;

        private BuffSystem() { TrySubscribe(); }

        public void TrySubscribe()
        {
            if (_subscribed) return;
            if (!ServiceLocator.Contains<TimeUtil>()) return;
            ServiceLocator.Get<TimeUtil>().AddUpdateAction(Tick);
            _subscribed = true;
        }

        public void ApplyBuff(IReadOnlyUnit unit, Buff buff)
        {
            if (unit == null || buff == null) return;
            if (unit is not Unit concrete) return;
            if (!buff.CanApplyTo(concrete)) return;

            if (!_buffs.TryGetValue(unit, out var list))
            {
                list = new List<Buff>();
                _buffs[unit] = list;
            }

            buff.Apply(concrete);
            list.Add(buff);
        }

        public bool HasBuff(IReadOnlyUnit unit)
            => unit != null && _buffs.TryGetValue(unit, out var list) && list.Count > 0;

        public bool HasBuff(IReadOnlyUnit unit, System.Type buffType)
        {
            if (unit == null || !_buffs.TryGetValue(unit, out var list)) return false;
            foreach (var b in list)
                if (b.GetType() == buffType) return true;
            return false;
        }

        public void Clear()
        {
            foreach (var pair in _buffs)
                foreach (var b in pair.Value)
                    if (pair.Key is Unit u) b.Remove(u);
            _buffs.Clear();
        }

        private void Tick(float deltaTime)
        {
            foreach (var pair in _buffs)
            {
                var list = pair.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    list[i].Tick(deltaTime);
                    if (list[i].IsExpired)
                    {
                        if (pair.Key is Unit u) list[i].Remove(u);
                        list.RemoveAt(i);
                    }
                }
            }
        }
    }
}