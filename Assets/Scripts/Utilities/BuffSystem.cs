using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;

namespace Utilities
{
    // Управляет баффами для юнитов. Хранит маппинг от Unit -> List<Buff>
    public class BuffSystem : MonoBehaviour, IBuffSystem
    {
        private readonly Dictionary<Unit, List<Buff>> _buffs = new();
        private Coroutine _tickCoroutine;

        public static BuffSystem Create()
        {
            var go = new GameObject("BuffSystem");
            DontDestroyOnLoad(go);
            return go.AddComponent<BuffSystem>();
        }

        private void Awake()
        {
            // Запускаем корутину тика
            _tickCoroutine = StartCoroutine(TickCoroutine());
        }

        private IEnumerator TickCoroutine()
        {
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                Tick(0.1f);
                yield return wait;
            }
        }

        private void OnDestroy()
        {
            if (_tickCoroutine != null)
                StopCoroutine(_tickCoroutine);
        }

        private void Tick(float delta)
        {
            var toRemoveUnits = new List<Unit>();
            foreach (var kv in _buffs)
            {
                var list = kv.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    list[i].Tick(delta);
                    if (list[i].IsExpired)
                        list.RemoveAt(i);
                }

                if (list.Count == 0)
                    toRemoveUnits.Add(kv.Key);
            }

            foreach (var u in toRemoveUnits)
                _buffs.Remove(u);
        }

        public void AddBuff(Unit unit, Buff buff)
        {
            if (!_buffs.TryGetValue(unit, out var list))
            {
                list = new List<Buff>();
                _buffs[unit] = list;
            }

            list.Add(buff);
        }

        // Возвращает умножитель для задержки движения. По умолчанию 1f (нет изменений).
        public float GetMovementMultiplier(Unit unit)
        {
            if (!_buffs.TryGetValue(unit, out var list) || list.Count == 0)
                return 1f;

            float result = 1f;
            foreach (var b in list)
                result *= b.MovementMultiplier;
            return result;
        }

        // Возвращает умножитель для задержки атаки.
        public float GetAttackMultiplier(Unit unit)
        {
            if (!_buffs.TryGetValue(unit, out var list) || list.Count == 0)
                return 1f;

            float result = 1f;
            foreach (var b in list)
                result *= b.AttackMultiplier;
            return result;
        }

        // Удобные методы для создания стандартных баффов
        public void AddSpeedBuff(Unit unit, float duration)
        {
            AddBuff(unit, new Buff(duration, movementMultiplier: 0.5f));
        }

        public void AddSlowDebuff(Unit unit, float duration)
        {
            AddBuff(unit, new Buff(duration, movementMultiplier: 1.5f));
        }

        public void AddAttackSpeedBuff(Unit unit, float duration)
        {
            AddBuff(unit, new Buff(duration, attackMultiplier: 0.5f));
        }

        public void AddAttackSlowDebuff(Unit unit, float duration)
        {
            AddBuff(unit, new Buff(duration, attackMultiplier: 1.5f));
        }
    }
}
