using System.Collections.Generic;
using System.Linq;
using Utilities;
using Model.Runtime;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Assets.Scripts.Model.Runtime.Buffs
{
    public sealed class BuffSystem
    {
        private readonly Dictionary<Unit, List<IUnitBuff>> _buffs = new();
        private bool _isProcessing;

        public BuffSystem(TimeUtil timeUtil)
        {
            timeUtil.AddFixedUpdateAction(UpdateAll);
        }
        public void StartProcessing() => _isProcessing = true;
        public void StopProcessing() => _isProcessing = false;

        public void AddBuff(Unit unit, IUnitBuff buff)
        {
            if (!buff.CanApply(unit))
                return;
            if (!_buffs.TryGetValue(unit, out var list))
                _buffs[unit] = list = new();
            if (list.Any(b => b.GetType() == buff.GetType()))
                return;

            list.Add(buff);
            buff.ApplyTo(unit);
#if UNITY_EDITOR
            Debug.Log($"<color=#00ff88>[BUFF ON]</color> {buff} ➜ {unit}");
#endif
        }

        public void RemoveAll(Unit unit)
        {
            if (!_buffs.TryGetValue(unit, out var list)) return;
            foreach (var b in list)
            {
                b.RemoveFrom(unit);
#if UNITY_EDITOR
                Debug.Log($"<color=#ff4444>[BUFF OFF]</color> {b} ✕ {unit}");
#endif
            }

            _buffs.Remove(unit);
        }

        private void UpdateAll(float dt)
        {
            if (!_isProcessing) return;

            foreach (var kvp in _buffs.ToList())
            {
                var unit = kvp.Key;
                var list = kvp.Value;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Tick(dt))
                    {
                        list[i].RemoveFrom(unit);
#if UNITY_EDITOR
                        Debug.Log($"<color=#ff4444>[BUFF OFF]</color> {list[i]} ✕ {unit}");
#endif
                        list.RemoveAt(i);
                    }
                }

                if (list.Count == 0)
                    _buffs.Remove(unit);
            }
        }
        public bool HasBuff<T>(Unit u) where T
            : IUnitBuff =>
            _buffs.TryGetValue(u, out var list)
            && list.Any(b => b is T);
    }
}