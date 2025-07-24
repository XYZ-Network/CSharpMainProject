using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Systems.Buffs
{
    public class BuffSystem : MonoBehaviour
    {
        private class ActiveBuff
        {
            public readonly Buff Buff;
            public readonly Coroutine Coroutine;

            public ActiveBuff(Buff buff, Coroutine coroutine)
            {
                Buff = buff;
                Coroutine = coroutine;
            }
        }

        private readonly Dictionary<IReadOnlyUnit, List<ActiveBuff>> _buffs = new();

        public void ApplyBuff(IReadOnlyUnit unit, Buff buff)
        {
            if (!_buffs.ContainsKey(unit))
            {
                _buffs[unit] = new List<ActiveBuff>();
            }

            var coroutine = StartCoroutine(BuffTimer(unit, buff));
            _buffs[unit].Add(new ActiveBuff(buff, coroutine));
        }

        private IEnumerator BuffTimer(IReadOnlyUnit unit, Buff buff)
        {
            float timeLeft = buff.Duration;

            while (timeLeft > 0)
            {
                // Preliminary cleanup in case of death
                if (unit == null || unit.IsDead)
                {
                    RemoveAllBuffs(unit);
                    yield break;
                }

                timeLeft -= Time.deltaTime; 

                // Wait till next frame.
                yield return null;
            }
            
            if (_buffs.TryGetValue(unit, out var list))
            {
                list.RemoveAll(b => b.Buff == buff);

                if (list.Count == 0)
                {
                    Debug.Log($"Buff {buff} expired");
                    _buffs.Remove(unit);
                }
            }
        }

        public float GetSpeedMultiplier(IReadOnlyUnit unit)
        {
            return !_buffs.TryGetValue(unit, out var buffs)
                ? 1f
                : buffs.Aggregate(1f, (acc, b) => acc * b.Buff.SpeedMultiplier);
        }

        public float GetAttackSpeedMultiplier(IReadOnlyUnit unit)
        {
            return !_buffs.TryGetValue(unit, out var buffs)
                ? 1f
                : buffs.Aggregate(1f, (acc, b) => acc * b.Buff.AttackSpeedMultiplier);
        }

        public bool HasAnyBuff(IReadOnlyUnit unit)
        {
            return _buffs.TryGetValue(unit, out var buffs) && buffs.Count > 0;
        }

        public void RemoveAllBuffs(IReadOnlyUnit unit)
        {
            if (_buffs.TryGetValue(unit, out var buffs))
            {
                foreach (var b in buffs)
                {
                    if (b.Coroutine != null)
                    {
                        StopCoroutine(b.Coroutine);                        
                    }
                }

                _buffs.Remove(unit);
            }
        }
    }
}