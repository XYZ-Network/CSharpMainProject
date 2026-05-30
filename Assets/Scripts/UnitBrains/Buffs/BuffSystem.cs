using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using UnityEngine;

namespace UnitBrains.Buffs
{
    public static class BuffSystem
    {
        private static Dictionary<Unit, List<IBuff>> _activeBuffs = new();

        public static void ApplyBuff(Unit target, IBuff buff)
        {
            if (!buff.CanApplyTo(target))
            {
                Debug.LogWarning($"Cannot apply {buff.BuffName} to {target.Config.Name}");
                return;
            }

            if (!_activeBuffs.ContainsKey(target))
                _activeBuffs[target] = new List<IBuff>();

            // Удаляем старый бафф того же типа
            var existing = _activeBuffs[target].FirstOrDefault(b => b.BuffName == buff.BuffName);
            if (existing != null)
            {
                existing.Remove(target);
                _activeBuffs[target].Remove(existing);
            }

            buff.Apply(target);
            _activeBuffs[target].Add(buff);
            Debug.Log($"Buff {buff.BuffName} applied to {target.Config.Name}");
        }

        public static void RemoveBuff(Unit target, IBuff buff)
        {
            if (_activeBuffs.ContainsKey(target) && _activeBuffs[target].Contains(buff))
            {
                buff.Remove(target);
                _activeBuffs[target].Remove(buff);
                Debug.Log($"Buff {buff.BuffName} removed from {target.Config.Name}");
            }
        }

        public static bool HasBuff(Unit target, string buffName)
        {
            return _activeBuffs.ContainsKey(target) &&
                   _activeBuffs[target].Any(b => b.BuffName == buffName);
        }

        public static void ClearAllBuffs(Unit target)
        {
            if (_activeBuffs.ContainsKey(target))
            {
                foreach (var buff in _activeBuffs[target])
                    buff.Remove(target);
                _activeBuffs[target].Clear();
            }
        }
    }
}