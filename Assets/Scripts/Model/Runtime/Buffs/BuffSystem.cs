using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.ReadOnly;
using UnityEngine;

namespace Model.Runtime.Buffs
{
    /// <summary>
    /// Хранит активные баффы юнитов и следит за их временем жизни.
    /// Один экземпляр системы регистрируется в ServiceLocator.
    /// </summary>
    public sealed class BuffSystem : MonoBehaviour, IBuffSystem
    {
        private readonly Dictionary<Unit, List<UnitBuff>> _buffsByUnit = new();
        private Coroutine _lifetimeCoroutine;

        public static BuffSystem Create()
        {
            var go = new GameObject(nameof(BuffSystem));
            DontDestroyOnLoad(go);
            var system = go.AddComponent<BuffSystem>();
            system._lifetimeCoroutine = system.StartCoroutine(system.UpdateBuffsCoroutine());
            return system;
        }

        public void AddBuff(Unit unit, UnitBuff buff)
        {
            if (unit == null)
                throw new System.ArgumentNullException(nameof(unit));

            if (buff == null)
                throw new System.ArgumentNullException(nameof(buff));

            if (!_buffsByUnit.TryGetValue(unit, out var buffs))
            {
                buffs = new List<UnitBuff>();
                _buffsByUnit.Add(unit, buffs);
            }

            buffs.Add(buff);
        }

        public bool RemoveBuff(Unit unit, UnitBuff buff)
        {
            if (unit == null || buff == null)
                return false;

            if (!_buffsByUnit.TryGetValue(unit, out var buffs))
                return false;

            var removed = buffs.Remove(buff);
            if (buffs.Count == 0)
                _buffsByUnit.Remove(unit);

            return removed;
        }

        public void ClearBuffs(Unit unit)
        {
            if (unit != null)
                _buffsByUnit.Remove(unit);
        }

        public float GetMoveSpeedMultiplier(IReadOnlyUnit unit)
        {
            return GetMultiplier(unit, buff => buff.MoveSpeedMultiplier);
        }

        public float GetAttackSpeedMultiplier(IReadOnlyUnit unit)
        {
            return GetMultiplier(unit, buff => buff.AttackSpeedMultiplier);
        }

        public bool HasBuff(IReadOnlyUnit unit)
        {
            return GetBuffs(unit).Count > 0;
        }

        public IReadOnlyList<UnitBuff> GetBuffs(IReadOnlyUnit unit)
        {
            if (unit is Unit concreteUnit &&
                _buffsByUnit.TryGetValue(concreteUnit, out var buffs))
            {
                return buffs;
            }

            return System.Array.Empty<UnitBuff>();
        }

        private float GetMultiplier(
            IReadOnlyUnit readOnlyUnit,
            System.Func<UnitBuff, float> selector)
        {
            if (!(readOnlyUnit is Unit unit) ||
                !_buffsByUnit.TryGetValue(unit, out var buffs))
            {
                return 1f;
            }

            // Модификаторы складываются мультипликативно.
            return buffs.Aggregate(1f, (result, buff) => result * selector(buff));
        }

        private IEnumerator UpdateBuffsCoroutine()
        {
            while (true)
            {
                yield return null;

                var expiredUnits = new List<Unit>();

                foreach (var pair in _buffsByUnit)
                {
                    var buffs = pair.Value;

                    for (int i = buffs.Count - 1; i >= 0; i--)
                    {
                        buffs[i].Tick(Time.deltaTime);

                        if (buffs[i].IsExpired)
                            buffs.RemoveAt(i);
                    }

                    if (buffs.Count == 0)
                        expiredUnits.Add(pair.Key);
                }

                foreach (var unit in expiredUnits)
                    _buffsByUnit.Remove(unit);
            }
        }

        private void OnDestroy()
        {
            if (_lifetimeCoroutine != null)
                StopCoroutine(_lifetimeCoroutine);

            _buffsByUnit.Clear();
        }
    }
}
