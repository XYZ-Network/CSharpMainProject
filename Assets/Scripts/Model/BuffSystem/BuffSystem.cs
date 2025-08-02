// Scripts/Model/BuffSystem/BuffSystemManager.cs
using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

namespace Model.BuffSystem
{
    public class BuffSystemManager : MonoBehaviour  // Переименовали класс
    {
        private readonly Dictionary<IReadOnlyUnit, List<BuffDebuff>> _activeBuffs = new();

        private void Awake()
        {
            ServiceLocator.RegisterAs(this, typeof(BuffSystemManager));
        }

        private void Update()
        {
            UpdateBuffs(Time.deltaTime);
        }

        public void ApplyBuff(IReadOnlyUnit unit, BuffDebuff buff)
        {
            if (!_activeBuffs.ContainsKey(unit))
                _activeBuffs[unit] = new List<BuffDebuff>();

            _activeBuffs[unit].Add(buff);
        }

        public float GetModifier(IReadOnlyUnit unit, BuffType type)
        {
            if (!_activeBuffs.TryGetValue(unit, out var buffs))
                return 1f;

            float modifier = 1f;
            foreach (var buff in buffs)
                if (buff.Type == type)
                    modifier *= buff.Modifier;

            return modifier;
        }

        private void UpdateBuffs(float deltaTime)
        {
            var expiredUnits = new List<IReadOnlyUnit>();

            foreach (var kvp in _activeBuffs)
            {
                kvp.Value.ForEach(b => b.Update(deltaTime));
                kvp.Value.RemoveAll(b => b.IsExpired);

                if (kvp.Value.Count == 0)
                    expiredUnits.Add(kvp.Key);
            }

            foreach (var unit in expiredUnits)
                _activeBuffs.Remove(unit);
        }
    }
}
