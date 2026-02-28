using System.Collections.Generic;
using UnityEngine;

namespace Model.Runtime
{
    public class BuffSystem
    {
        private readonly Dictionary<Unit, List<IBuff>> _unitBuffs = new();

        public void AddBuff(Unit unit, IBuff buff)
        {
            if (!buff.CanApply(unit))
                return;

            if (!_unitBuffs.ContainsKey(unit))
                _unitBuffs[unit] = new List<IBuff>();

            _unitBuffs[unit].Add(buff);
            buff.ApplyNonGeneric(unit);
            Debug.Log($"[BuffSystem] Баффнут {unit.Config.Name}: {buff.GetDescription()}");
        }

        public void RemoveUnit(Unit unit)
        {
            if (_unitBuffs.TryGetValue(unit, out var buffs))
            {
                foreach (var buff in buffs)
                    buff.RemoveNonGeneric(unit);

                _unitBuffs.Remove(unit);
            }
        }

        // Проверить, есть ли у юнита активный бафф конкретного типа
        public bool HasActiveBuff<TBuff>(Unit unit) where TBuff : IBuff
        {
            if (!_unitBuffs.TryGetValue(unit, out var buffs))
                return false;

            foreach (var buff in buffs)
                if (buff is TBuff)
                    return true;

            return false;
        }

        public void Update(float deltaTime)
        {
            var unitsToClean = new List<Unit>();

            foreach (var kvp in _unitBuffs)
            {
                var unit = kvp.Key;
                var buffs = kvp.Value;
                var buffsToRemove = new List<IBuff>();

                foreach (var buff in buffs)
                {
                    buff.Duration -= deltaTime;
                    if (buff.Duration <= 0)
                        buffsToRemove.Add(buff);
                }

                foreach (var buff in buffsToRemove)
                {
                    buff.RemoveNonGeneric(unit);
                    buffs.Remove(buff);
                    Debug.Log($"[BuffSystem] Убран бафф {unit.Config.Name}: {buff.GetDescription()}");
                }

                if (buffs.Count == 0)
                    unitsToClean.Add(unit);
            }

            foreach (var unit in unitsToClean)
                _unitBuffs.Remove(unit);
        }

        public void Clear()
        {
            foreach (var kvp in _unitBuffs)
                foreach (var buff in kvp.Value)
                    buff.RemoveNonGeneric(kvp.Key);

            _unitBuffs.Clear();
        }
    }
}