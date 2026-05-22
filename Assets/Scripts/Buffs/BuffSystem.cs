using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Buffs
{
    public sealed class BuffSystem
    {
        private const float TickDelay = 0.1f;

        private readonly Dictionary<IReadOnlyUnit, List<UnitBuff>> _buffs = new();
        private readonly TimeUtil _timeUtil;
        private readonly Coroutine _coroutine;

        public BuffSystem(TimeUtil timeUtil)
        {
            _timeUtil = timeUtil;
            _coroutine = _timeUtil.StartCoroutine(UpdateCoroutine());
        }

        public void AddBuff(IReadOnlyUnit unit, UnitBuff buff)
        {
            if (unit == null || buff == null)
                return;

            if (!_buffs.ContainsKey(unit))
                _buffs[unit] = new List<UnitBuff>();

            _buffs[unit].Add(buff);
        }

        public float GetMoveSpeedModifier(IReadOnlyUnit unit)
        {
            return GetModifier(unit, buff => buff.MoveSpeedModifier);
        }

        public float GetAttackSpeedModifier(IReadOnlyUnit unit)
        {
            return GetModifier(unit, buff => buff.AttackSpeedModifier);
        }

        public void Clear()
        {
            _buffs.Clear();
        }

        private float GetModifier(IReadOnlyUnit unit, Func<UnitBuff, float> selector)
        {
            if (unit == null)
                return 1f;

            if (!_buffs.ContainsKey(unit))
                return 1f;

            if (_buffs[unit].Count == 0)
                return 1f;

            float result = 1f;

            foreach (var buff in _buffs[unit])
            {
                result *= selector(buff);
            }

            return Mathf.Max(0.05f, result);
        }

        private IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(TickDelay);

                var units = _buffs.Keys.ToList();

                foreach (var unit in units)
                {
                    if (!_buffs.ContainsKey(unit))
                        continue;

                    var buffs = _buffs[unit];

                    for (int i = buffs.Count - 1; i >= 0; i--)
                    {
                        buffs[i].Tick(TickDelay);

                        if (buffs[i].IsFinished)
                            buffs.RemoveAt(i);
                    }

                    if (buffs.Count == 0)
                        _buffs.Remove(unit);
                }
            }
        }
    }
}