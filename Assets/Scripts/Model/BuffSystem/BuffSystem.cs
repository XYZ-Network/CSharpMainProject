using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Model.BuffSystem
{
    public class BuffSystemManager : MonoBehaviour
    {
        private readonly Dictionary<IReadOnlyUnit, List<IBuff>> _activeBuffs = new();

        private void Awake()
        {
            ServiceLocator.RegisterAs(this, typeof(BuffSystemManager));
        }

        private void Update()
        {
            UpdateBuffs(Time.deltaTime);
        }

        public void ApplyBuff(IReadOnlyUnit unit, IBuff buff)
        {
            if (!buff.CanApply(unit))
                return;

            if (!_activeBuffs.ContainsKey(unit))
                _activeBuffs[unit] = new List<IBuff>();

            _activeBuffs[unit].Add(buff);
            buff.Apply(unit);
        }

        public bool HasBuffs(IReadOnlyUnit unit)
        {
            return _activeBuffs.ContainsKey(unit) && _activeBuffs[unit].Count > 0;
        }

        private void UpdateBuffs(float deltaTime)
        {
            var expiredUnits = new List<IReadOnlyUnit>();

            foreach (var kvp in _activeBuffs)
            {
                foreach (var buff in kvp.Value)
                {
                    buff.Update(deltaTime);
                    if (buff.IsExpired)
                    {
                        buff.Reset(kvp.Key);
                    }
                }

                kvp.Value.RemoveAll(b => b.IsExpired);

                if (kvp.Value.Count == 0)
                    expiredUnits.Add(kvp.Key);
            }

            foreach (var unit in expiredUnits)
                _activeBuffs.Remove(unit);
        }
    }
}