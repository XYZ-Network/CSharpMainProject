using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public class BuffSystem
    {
        private readonly Dictionary<Unit, Buff> Buffs;
        private Coroutine buffsCoroutine;
        private readonly TimeUtil _timeUtil;
        public BuffSystem()
        {
            Buffs = new Dictionary<Unit, Buff>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
        }
        public void AddBuff(Unit unit, Buff buff)
        {
            if (!Buffs.ContainsKey(unit))
                Buffs.Add(unit, buff);
            else
                Buffs[unit] = buff;
            if (Buffs.Count > 0 && buffsCoroutine == null)
                buffsCoroutine = _timeUtil.StartCoroutine(BuffsCoroutine());
        }
        public IEnumerator BuffsCoroutine()
        {
            List<Unit> keysToDelete = new();
            while (true)
            {
                foreach (var buffPair in Buffs)
                {
                    var buff = buffPair.Value;
                    if (buff.Duration > 0)
                        buff.ReduceDuration();
                    if (buff.Duration < 0.1f)
                        keysToDelete.Add(buffPair.Key);
                }
                foreach (var key in keysToDelete)
                {
                    Buffs.Remove(key);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        public bool ContainsKey(Unit unit)
        {
            return Buffs.ContainsKey(unit);
        }
        public Buff GetBuff(Unit unit)
        {
            return Buffs[unit];
        }
        public float GetMoveMultiplier(Unit unit)
        {
            return Buffs[unit].MoveModifier;
        }
        public float GetAttackMultiplier(Unit unit)
        {
            return Buffs[unit].AttackModifier;
        }
        public void DeleteBuff(Unit unit)
        {
            Buffs.Remove(unit);
        }
    }
}
