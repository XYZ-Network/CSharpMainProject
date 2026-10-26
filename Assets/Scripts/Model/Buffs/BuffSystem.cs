using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using Utilities;
using UnityEngine;

namespace Model.Buffs
{
    public class BuffSystem
    {
        private static BuffSystem _instance;
        public static BuffSystem Instance => _instance ??= new BuffSystem();

        private readonly Dictionary<IReadOnlyUnit, List<Buff>> _buffs = new();
        private bool _subscribed;

        private BuffSystem() { TrySubscribe(); }

        /// <summary>
        /// Подписка на TimeUtil. Вызывается при создании.
        /// Если TimeUtil ещё не зарегистрирован, вызовем позже —
        /// при первом обращении к системе из Unit.
        /// </summary>
        public void TrySubscribe()
        {
            if (_subscribed) return;
            if (!ServiceLocator.Contains<TimeUtil>()) return;

            ServiceLocator.Get<TimeUtil>().AddUpdateAction(Tick);
            _subscribed = true;
        }

        // ---------- API ----------

        public void ApplyBuff(IReadOnlyUnit unit, Buff buff)
        {
            if (unit == null || buff == null) return;

            if (!_buffs.TryGetValue(unit, out var list))
            {
                list = new List<Buff>();
                _buffs[unit] = list;
            }
            list.Add(buff);
        }

        public bool HasBuff(IReadOnlyUnit unit)
            => unit != null
               && _buffs.TryGetValue(unit, out var list)
               && list.Count > 0;

        public bool HasBuff(IReadOnlyUnit unit, BuffType type)
        {
            if (unit == null) return false;
            if (!_buffs.TryGetValue(unit, out var list)) return false;

            foreach (var b in list)
                if (b.Type == type) return true;
            return false;
        }

        /// <summary>
        /// Итоговый множитель скорости движения (перемножаем все эффекты).
        /// </summary>
        public float GetMoveSpeedMultiplier(IReadOnlyUnit unit)
        {
            if (unit == null || !_buffs.TryGetValue(unit, out var list))
                return 1f;

            float m = 1f;
            foreach (var b in list) m *= b.MoveSpeedMultiplier;
            return m;
        }

        /// <summary>
        /// Итоговый множитель скорости атаки.
        /// </summary>
        public float GetAttackSpeedMultiplier(IReadOnlyUnit unit)
        {
            if (unit == null || !_buffs.TryGetValue(unit, out var list))
                return 1f;

            float m = 1f;
            foreach (var b in list) m *= b.AttackSpeedMultiplier;
            return m;
        }

        public void Clear() => _buffs.Clear();

        // ---------- Тик ----------

        private void Tick(float deltaTime)
        {
            foreach (var pair in _buffs)
            {
                var list = pair.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    list[i].TimeLeft -= deltaTime;
                    if (list[i].IsExpired)
                        list.RemoveAt(i);
                }
            }
        }
    }
}