using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuffSystem
{
    public class BuffManager : IBuffSystem
    {
        private readonly Dictionary<IModifiableUnit, List<Buff<IModifiableUnit>>> _unitBuffs =
            new Dictionary<IModifiableUnit, List<Buff<IModifiableUnit>>>();
        private MonoBehaviour _context;

        public void Initialize(MonoBehaviour context)
        {
            _context = context;
            _context.StartCoroutine(UpdateBuffs());
        }

        private IEnumerator UpdateBuffs()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f); // обновление каждые 0.1 с
                UpdateAllBuffs();
            }
        }

        private void UpdateAllBuffs()
        {
            var unitsToRemove = new List<IModifiableUnit>();

            foreach (var kvp in _unitBuffs)
            {
                var unit = kvp.Key;
                var buffsToRemove = new List<Buff<IModifiableUnit>>();

                foreach (var buff in kvp.Value)
                {
                    buff.Update(0.1f); // Обновляем бафф — он сам проверяет IsExpired

                    // Проверяем, истёк ли бафф после обновления
                    if (buff.IsExpired)
                    {
                        buffsToRemove.Add(buff);
                    }
                }

                // Удаляем истёкшие баффы
                foreach (var expiredBuff in buffsToRemove)
                {
                    RemoveBuffInternal(unit, expiredBuff.Type);
                }

                // Если у юнита не осталось баффов, помечаем его для удаления
                if (kvp.Value.Count == 0)
                {
                    unitsToRemove.Add(unit);
                }
            }

            // Очищаем словарь от юнитов без баффов
            foreach (var unit in unitsToRemove)
            {
                _unitBuffs.Remove(unit);
            }
        }

        private void RemoveBuffInternal(IModifiableUnit unit, BuffType type)
        {
            if (_unitBuffs.TryGetValue(unit, out var buffs))
            {
                var buffToRemove = buffs.FirstOrDefault(b => b.Type == type);
                if (buffToRemove != null)
                {
                    buffToRemove.Remove();
                    buffs.Remove(buffToRemove);
                }
            }
        }

        public void AddBuff(IModifiableUnit unit, BuffType type, float duration, float modifier)
        {
            Buff<IModifiableUnit> buff = null;

            // Создаём бафф через switch
            switch (type)
            {
                case BuffType.MoveSpeed:
                    buff = new MoveSpeedBuff(duration, modifier);
                    break;
                case BuffType.AttackSpeed:
                    buff = new AttackSpeedBuff(duration, modifier);
                    break;
                case BuffType.AttackRange:
                    buff = new AttackRangeBuff(duration, modifier);
                    break;
                case BuffType.AttackCount:
                    buff = new AttackCountBuff(duration, modifier);
                    break;
                default:
                    throw new ArgumentException($"Unsupported buff type: {type}");
            }

            if (buff.CanApply(unit))
            {
                buff.Apply(unit);

                if (!_unitBuffs.ContainsKey(unit))
                {
                    _unitBuffs[unit] = new List<Buff<IModifiableUnit>>();
                }

                // Удаляем существующий бафф того же типа
                var existingBuff = _unitBuffs[unit].FirstOrDefault(b => b.Type == type);
                if (existingBuff != null)
                {
                    _unitBuffs[unit].Remove(existingBuff);
                }

                // Добавляем новый бафф
                _unitBuffs[unit].Add(buff);
            }
        }

        public bool HasAnyBuff(Unit unit)
        {
            var modifiableUnit = unit as IModifiableUnit;
            return _unitBuffs.ContainsKey(modifiableUnit) &&
                   _unitBuffs[modifiableUnit].Count > 0;
        }
    }
}
