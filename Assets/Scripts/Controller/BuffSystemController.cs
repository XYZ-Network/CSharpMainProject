using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using Effects;
using UnityEngine;
using Utilities;

namespace Controller
{
    public class BuffSystemController
    {
        // Для хранения баффов рекомендуется использовать словарь. 
		// Словарь юнит - бафф
		private Dictionary<IReadOnlyUnit, List<Buff>> buffs = new Dictionary<IReadOnlyUnit, List<Buff>>();
		private readonly TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();

		public float GetMoveDelayModifier(IReadOnlyUnit unit) 
        {
			if (!buffs.ContainsKey(unit)) return 1f;
            
			float result = 1f;
            foreach (var buff in buffs[unit])
                result *= buff.moveDelayModifier;
            return result;
        }

		public float GetAttackDelayModifier(IReadOnlyUnit unit) 
        {
			if (!buffs.ContainsKey(unit)) return 1f;

            float result = 1f;
            foreach (var buff in buffs[unit])
                result *= buff.attackDelayModifier;
            return result;
        }

        public void AddBuff(IReadOnlyUnit unit, Buff buff)
        {
            if (!buffs.ContainsKey(unit))
                buffs[unit] = new List<Buff>();
            
            buffs[unit].Add(buff);
            _timeUtil.RunDelayed(buff.duration, () => RemoveBuff(unit, buff));
        }

        private void RemoveBuff(IReadOnlyUnit unit, Buff buff)
        {
            if (buffs.ContainsKey(unit))
                buffs[unit].Remove(buff);
        }

        public bool HasBuff(IReadOnlyUnit unit)
        {
            return buffs.ContainsKey(unit) && buffs[unit].Count > 0;
        }
    }
}