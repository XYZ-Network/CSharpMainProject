using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Runtime.Buffs
{
    public sealed class BuffSystem : MonoBehaviour, IBuffSystem
    {
        private readonly Dictionary<Unit, List<IBuff>> _buffsByUnit = new();

        private Coroutine _lifetimeCoroutine;

        public static BuffSystem Create()
        {
            var go = new GameObject(nameof(BuffSystem));
            DontDestroyOnLoad(go);

            var system = go.AddComponent<BuffSystem>();

            system._lifetimeCoroutine =
                system.StartCoroutine(system.UpdateBuffsCoroutine());

            return system;
        }

        public bool AddBuff(Unit unit, IBuff buff)
        {
            if (unit == null || buff == null)
                return false;

            if (!buff.CanApply(unit))
                return false;

            if (!_buffsByUnit.TryGetValue(unit, out var buffs))
            {
                buffs = new List<IBuff>();
                _buffsByUnit.Add(unit, buffs);
            }

            if (buffs.Exists(x => x.Id == buff.Id))
                return false;

            buff.Apply(unit);
            buffs.Add(buff);

            return true;
        }

        public bool HasBuff(Unit unit)
        {
            return unit != null &&
                   _buffsByUnit.TryGetValue(unit, out var buffs) &&
                   buffs.Count > 0;
        }

        public void ClearBuffs(Unit unit)
        {
            if (unit == null)
                return;

            if (!_buffsByUnit.TryGetValue(unit, out var buffs))
                return;

            foreach (var buff in buffs)
                buff.Remove(unit);

            _buffsByUnit.Remove(unit);
        }

        private IEnumerator UpdateBuffsCoroutine()
        {
            while (true)
            {
                yield return null;

                var emptyUnits = new List<Unit>();

                foreach (var pair in _buffsByUnit)
                {
                    var unit = pair.Key;
                    var buffs = pair.Value;

                    for (var i = buffs.Count - 1; i >= 0; i--)
                    {
                        var buff = buffs[i];

                        buff.Update(Time.deltaTime);

                        if (!buff.IsExpired)
                            continue;

                        buff.Remove(unit);
                        buffs.RemoveAt(i);
                    }

                    if (buffs.Count == 0)
                        emptyUnits.Add(unit);
                }

                foreach (var unit in emptyUnits)
                    _buffsByUnit.Remove(unit);
            }
        }

        private void OnDestroy()
        {
            if (_lifetimeCoroutine != null)
                StopCoroutine(_lifetimeCoroutine);

            foreach (var pair in _buffsByUnit)
            {
                foreach (var buff in pair.Value)
                    buff.Remove(pair.Key);
            }

            _buffsByUnit.Clear();
        }
    }
}