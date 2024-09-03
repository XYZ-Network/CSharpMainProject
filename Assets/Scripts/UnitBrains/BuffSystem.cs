using Buffs.Buffs;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;
using View;

namespace Buffs
{
    public class BuffSystem
    {
        private VFXView _vfxView => ServiceLocator.Get<VFXView>();

     private readonly TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();
     public readonly Dictionary<IReadOnlyUnit, HashSet<Type>> unitBuffs = new();
     internal List<Buff<BaseUnitBrain>> availableBuffs = new List<Buff<BaseUnitBrain>>();
        public BuffSystem()
        {
            // Инициализация доступных баффов
            availableBuffs.Add(new MovementBuff<BaseUnitBrain>(1f));
            availableBuffs.Add(new AttackSpeedBuff<BaseUnitBrain>(1f));
            availableBuffs.Add(new DoubleShootBuff<BaseUnitBrain>(1f));
            availableBuffs.Add(new RadiusBuff<BaseUnitBrain>(1f));
        }

        public void ApplyBuff(IReadOnlyUnit unit, Buff<BaseUnitBrain> buff) 
        {
            if (!unitBuffs.ContainsKey(unit))
            {
                unitBuffs[unit] = new HashSet<Type>();
            }
            if (buff.CanApply(UnitBrainProvider.GetBrain(unit.Config))) {
                unitBuffs[unit].Add(buff.GetType());
                buff.Apply(unit);
                Debug.Log($"Apply buff {buff.GetType().Name}, unit type is --- {unit.Config.name}");
                _vfxView.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
                _timeUtil.RunDelayed(buff.Duration, () => UpdateBuffs(unit, buff));
            }
            else
            {
                Debug.Log($"Can't apply buff {buff.GetType().Name}, brain type is --- {UnitBrainProvider.GetBrain(unit.Config).GetType().Name}");
            }
        }

        public void UpdateBuffs<TBrain>(IReadOnlyUnit unit, Buff<TBrain> buff) where TBrain : BaseUnitBrain
        {
            buff.Expire(unit);
            unitBuffs[unit].Remove(buff.GetType());

        }
    }
}
