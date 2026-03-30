using Assets.Scripts.Model.Runtime.Buff;
using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using View;

namespace Assets.Scripts.UnitBrains
{
    public class BuffManager : MonoBehaviour
    {
        private List<IReadOnlyBuff> _buffList;
        private readonly Dictionary<IReadOnlyUnit, Dictionary<BuffType, IReadOnlyBuff>> buffs = new();
        private List<Coroutine> activeBuffs = new();
        private VFXView _vfxView;
        private System.Random _random = new System.Random();

        public BuffManager() {
            _buffList = new()
            {
                new AttackSpeedBuff(),
                new MoveSpeedBuff()
            };
        }

        public void Clear() 
        {
            buffs.Clear();
            foreach (Coroutine c in activeBuffs)
            {
                StopCoroutine(c);
            }
        }


        public bool existByUnit(IReadOnlyUnit unit)
        {
            return buffs.ContainsKey(unit);
        }

        public void AddRandomBuff(IReadOnlyUnit unit)
        {
            int buffIndex = _random.Next(0, _buffList.Count);
            AddBuff(unit, _buffList[buffIndex]);
        }

        public void AddBuff(IReadOnlyUnit unit, IReadOnlyBuff buff)
        {
            if (buffs.ContainsKey(unit))
            {
                var unitBuffs = buffs[unit];
                if (!unitBuffs.ContainsKey(buff.Type))
                {
                    unitBuffs.Add(buff.Type, buff);
                }
            }
            else
            {
                var unitBuffs = new Dictionary<BuffType, IReadOnlyBuff>() 
                {
                    { buff.Type, buff }
                };
                buffs.Add(unit, unitBuffs);
            }
            GetVFXView().PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
            var cor = StartCoroutine(BuffTimerCoroutine(unit, buff));
            activeBuffs.Add(cor);
        }


        private IEnumerator BuffTimerCoroutine(IReadOnlyUnit unit, IReadOnlyBuff buff)
        {
            var currentDuration = 0;
            while (currentDuration <= buff.Duration)
            {
                currentDuration++;
                yield return new WaitForSeconds(1f);
            }
            buffs[unit].Remove(buff.Type);
        }

        private VFXView GetVFXView()
        {
            if (_vfxView == null)
            {
                _vfxView = ServiceLocator.Get<VFXView>();
            }
            return _vfxView;
        }



        public IReadOnlyBuff GetBuff(IReadOnlyUnit unit, BuffType buffType) 
        {
            if (!buffs.ContainsKey(unit))
            {
                return null;
            }
            var unitBuffs = buffs[unit];
            if (!unitBuffs.ContainsKey(buffType))
            {
                return null;
            }
            return unitBuffs[buffType];

        }

    }
}