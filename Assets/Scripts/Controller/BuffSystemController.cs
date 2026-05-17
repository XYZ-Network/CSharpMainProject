using System.Collections.Generic;
using Model.Runtime;
using Effects;
using UnityEngine;
using Utilities;

namespace Controller
{
    public class BuffSystemController
    {
        // Для хранения баффов рекомендуется использовать словарь. 
		// Словарь юнит - бафф
		private Dictionary<Unit, List<Buff>> buffs = new Dictionary<Unit, List<Buff>>();
		private readonly TimeUtil _timeUtil = ServiceLocator.Get<TimeUtil>();

		public float GetMoveDelayModifier(Unit unit) 
        {
			if (!buffs.ContainsKey(unit)) return 1f;
            
			float result = 1f;
            foreach (var buff in buffs[unit])
                result *= buff.moveDelayModifier;
            return result;
        }

		public float GetAttackDelayModifier(Unit unit) 
        {
			if (!buffs.ContainsKey(unit)) return 1f;

            float result = 1f;
            foreach (var buff in buffs[unit])
                result *= buff.attackDelayModifier;
            return result;
        }

        public void AddBuff(Unit unit, Buff buff)
        {
            if (!buffs.ContainsKey(unit))
                buffs[unit] = new List<Buff>();
            
            buffs[unit].Add(buff);
            _timeUtil.RunDelayed(buff.duration, () => RemoveBuff(unit, buff));
        }

        private void RemoveBuff(Unit unit, Buff buff)
        {
            if (buffs.ContainsKey(unit))
                buffs[unit].Remove(buff);
        }
    }
}