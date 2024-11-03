using Model.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Model.Runtime.Effects
{
    public class EffectManager
    {
        public ConcurrentDictionary<Unit, List<Effect>> _appliedEffectsToUnit = new ConcurrentDictionary<Unit, List<Effect>>();

        public float GetAttackDelayModifier(Unit unit)
        {
            if (_appliedEffectsToUnit.TryGetValue(unit, out var effects))
            {
                var attackModifiers = effects.Select(e => e.AttackDelayModifier);

                var result = 1f;

                foreach (var attackModifier in attackModifiers)
                {
                    result *= attackModifier;
                }

                return result;
            }
            else
            {
                return 1;
            }
        }

        public float GetMoveDelayModifier(Unit unit)
        {
            if (_appliedEffectsToUnit.TryGetValue(unit, out var effects))
            {
                var moveDelayModifiers = effects.Select(e => e.MoveDelayModifier);

                var result = 1f;

                foreach (var moveDelayModifier in moveDelayModifiers)
                {
                    result *= moveDelayModifier;
                }

                return result;
            }
            else
            {
                return 1;
            }
        }

        public void AddEffect<TEffect>(Unit unit)
            where TEffect : Effect, new()
        {
            var effectsList = new List<Effect>();

            _appliedEffectsToUnit.GetOrAdd(unit, effectsList);

            effectsList.Add(new TEffect());
        }

        public void UpdateEffectsDuration(Unit unit)
        {
            var time = Time.deltaTime;

            if (_appliedEffectsToUnit.TryGetValue(unit, out var effectsList))
            {
                foreach (var effect in effectsList)
                {
                    effect.Duration -= time;
                }

                var effectsToRemove = effectsList.Where(e => e.Duration <= 0);

                foreach (var effectToRemove in effectsToRemove)
                {
                    effectsList.Remove(effectToRemove);
                }
            }
        }

        public void RemoveAllEffects(Unit unit)
        {
            if (_appliedEffectsToUnit.ContainsKey(unit))
            {
                _appliedEffectsToUnit.TryRemove(unit, out var _);
            }
        }
    }
}