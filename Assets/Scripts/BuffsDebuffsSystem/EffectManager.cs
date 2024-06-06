using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Assets.Scripts.BuffsDebuffsSystem
{
    public class EffectManager : IDisposable
    {
        private TimeUtil _timeUtil;

        private Dictionary<IReadOnlyUnit, List<Effect>> _unitsEffects = new Dictionary<IReadOnlyUnit, List<Effect>>();


        public EffectManager()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();

            _timeUtil.AddFixedUpdateAction(updateEffects);
        }

        public void AddEffect(IReadOnlyUnit unit, Effect effect)
        {
            if (!_unitsEffects.ContainsKey(unit))
            {
                _unitsEffects[unit] = new List<Effect>();
            }

            _unitsEffects[unit].Add(effect);
        }

        public void RemoveEffect(IReadOnlyUnit unit, Effect effect)
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

        public float GetModifier(IReadOnlyUnit unit)
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

        public bool HasActiveBuffs(IReadOnlyUnit unit)
        {
            return _unitsEffects.ContainsKey(unit) && _unitsEffects[unit].Count > 0;
        }
    }
}
