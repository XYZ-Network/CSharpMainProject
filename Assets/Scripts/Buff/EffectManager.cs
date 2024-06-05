using Buff;
using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Buff
{
    public class EffectManager : IDisposable
    {
        private TimeUtil _timeUtil;

        private Dictionary<Unit, List<Effect>> _unitsEffects = new Dictionary<Unit, List<Effect>>();


        public EffectManager()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddFixedUpdateAction(updateEffects);
        }

        public void AddEffect(Unit unit, Effect effect)
        {
            if (!_unitsEffects.ContainsKey(unit))
            {
                _unitsEffects[unit] = new List<Effect>();
            }

            _unitsEffects[unit].Add(effect);
        }

        public void RemoveEffect(Unit unit, Effect effect)
        {
            if (_unitsEffects.ContainsKey(unit))
            {
                _unitsEffects[unit].Remove(effect);
            }
        }

        public void updateEffects(float deLtaTime)
        {
            foreach (var unitEffects in _unitsEffects)
            {
                foreach (Effect effect in unitEffects.Value.ToArray())
                {
                    effect.Duration -= deLtaTime;
                    if (effect.Duration <= 0)
                        RemoveEffect(unitEffects.Key, effect);
                }
            }
        }

        public float GetModifier(Unit unit)
        {
            float modifier = 1f;

            if (_unitsEffects.ContainsKey(unit))
            {
                foreach (Effect effect in _unitsEffects[unit])
                {
                    modifier *= effect.Modifier;
                }
            }

            return modifier;
        }

        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(updateEffects);
        }
    }
}