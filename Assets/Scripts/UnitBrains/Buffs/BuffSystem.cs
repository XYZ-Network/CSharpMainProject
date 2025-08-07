using Assets.Scripts.UnitBrains.Buffs;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.UnitBrains
{
    public class BuffSystem
    {
        private readonly Dictionary<Unit, Buff<BaseUnitBrain>> Buffs;
        private readonly Dictionary<Unit, Buff<BaseUnitBrain>> Debuffs;
        private Coroutine buffsCoroutine;
        private readonly TimeUtil _timeUtil;
        public BuffSystem()
        {
            Buffs = new Dictionary<Unit, Buff<BaseUnitBrain>>();
            Debuffs = new Dictionary<Unit, Buff<BaseUnitBrain>>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
        }
        public void AddBuff(Unit unit, Buff<BaseUnitBrain> buff)
        {
            if (buff.CanBeAppliedTo(unit))
            {
                if (!buff.IsBuff)
                {
                    if (Debuffs.ContainsKey(unit))
                        return;
                    Debuffs.Add(unit, buff);
                    Debuffs[unit].ApplyEffect(unit);
                    if (Buffs.Count > 0 && Debuffs.Count > 0 && buffsCoroutine == null)
                        buffsCoroutine = _timeUtil.StartCoroutine(BuffsCoroutine());
                }
                else
                {
                    if (Buffs.ContainsKey(unit))
                        return;
                    Buffs.Add(unit, buff);
                    Buffs[unit].ApplyEffect(unit);
                    if (Buffs.Count > 0 && Debuffs.Count > 0 && buffsCoroutine == null)
                        buffsCoroutine = _timeUtil.StartCoroutine(BuffsCoroutine());
                }
            }
            else
            {
                Debug.LogError("This type of Buff can not be Applied to this type of Unit.");
            }
        }
        public IEnumerator BuffsCoroutine()
        {
            List<Unit> keysBuffsToDelete = new();
            List<Unit> keysDebuffsToDelete = new();
            while (true)
            {
                foreach (var buffPair in Buffs)
                {
                    var buff = buffPair.Value;
                    if (buff.Duration > 0)
                        buff.ReduceDuration();
                    if (buff.Duration < 0.1f)
                        keysBuffsToDelete.Add(buffPair.Key);
                }
                foreach (var debuffPair in Debuffs)
                {
                    var debuff = debuffPair.Value;
                    if (debuff.Duration > 0)
                        debuff.ReduceDuration();
                    if (debuff.Duration < 0.1f)
                        keysDebuffsToDelete.Add(debuffPair.Key);
                }
                foreach (var key in keysBuffsToDelete)
                {
                    DeleteBuff(key);
                }
                foreach (var key in keysDebuffsToDelete)
                {
                    DeleteDebuff(key);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        public bool BuffsContainsKey(Unit unit)
        {
            return Buffs.ContainsKey(unit);
        }
        public bool DebuffsContainsKey(Unit unit)
        {
            return Debuffs.ContainsKey(unit);
        }
        public Buff<BaseUnitBrain> GetBuff(Unit unit)
        {
            return Buffs[unit];
        }
        public Buff<BaseUnitBrain> GetDebuff(Unit unit)
        {
            return Debuffs[unit];
        }
        public void DeleteBuff(Unit unit)
        {
            if (BuffsContainsKey(unit))
            {
                Buffs[unit].RemoveEffect(unit);
                Buffs.Remove(unit);
            }
        }
        public void DeleteDebuff(Unit unit)
        {
            if (DebuffsContainsKey(unit))
            {
                Debuffs[unit].RemoveEffect(unit);
                Debuffs.Remove(unit);
            }
        }
    }
}
