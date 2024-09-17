using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnityEngine;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class BuffAndDebuffControllSystem
    {
        private TimeUtil _timeUtil;
        private Dictionary<Unit, (List<Effect<Unit>> attackEffects, List<Effect<Unit>> moveEffects)> _effectsDictionary = new Dictionary<Unit, (List<Effect<Unit>> attackEffects, List<Effect<Unit>> moveEffect)>();

        public BuffAndDebuffControllSystem()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(UpdateEffects);
        }
        public void AddItem(Unit unit, Effect<Unit> effect)
        {
            if (!_effectsDictionary.ContainsKey(unit))
            {
                (List<Effect<Unit>> attackEffects, List<Effect<Unit>> moveEffect) newTupple = (new List<Effect<Unit>>(), new List<Effect<Unit>>());
                _effectsDictionary[unit] = newTupple;
            }
            switch (effect.EffectType)
            {
                case EffectType.Attack:
                    _effectsDictionary[unit].attackEffects.Add(effect);
                    break;
                case EffectType.Move:
                    _effectsDictionary[unit].moveEffects.Add(effect);
                    break;
               
            }
        }

        public void RemoveItem(Unit unit, Effect<Unit> effect)
        {
            if (_effectsDictionary.ContainsKey(unit))
            {
                switch (effect.EffectType)
                {
                    case EffectType.DAttack:
                        effect.ClearEffect(unit);
                        _effectsDictionary[unit].attackEffects.Remove(effect);
                        break;
                    case EffectType.DMove:
                        effect.ClearEffect(unit);
                        _effectsDictionary[unit].moveEffects.Remove(effect);
                        break;
                }
            }
        }

        public bool CheckUnitInEffectList(Unit unit)
        {
            return _effectsDictionary.ContainsKey(unit) ? true : false;
        }

        public (float attackMod, float moveMod) GetActualModifier(Unit unit)
        {
            float initialAttackMod = 0f;
            float initialMoveMod = 0f;
            if (_effectsDictionary.ContainsKey(unit))
            {
                if (_effectsDictionary[unit].attackEffects.Count > 0)
                {
                    foreach (var item in _effectsDictionary[unit].attackEffects)
                    {
                        initialAttackMod += item.Modifier;
                    }
                }
                if (_effectsDictionary[unit].moveEffects.Count > 0)
                {
                    foreach (var item in _effectsDictionary[unit].moveEffects)
                    {
                        initialMoveMod += item.Modifier;
                    }
                }
            }

            return (initialAttackMod == 0 ? 1 : initialAttackMod, initialMoveMod == 0 ? 1 : initialMoveMod);
        }

        public void UpdateEffects(float deLtaTime)
        {
            foreach (var unitEffects in _effectsDictionary)
            {
                foreach (Effect<Unit> effect in unitEffects.Value.attackEffects.ToArray())
                {
                    ManageLifeEffect(effect, deLtaTime, unitEffects);
                }
                foreach (Effect<Unit> effect in unitEffects.Value.moveEffects.ToArray())
                {
                    ManageLifeEffect(effect, deLtaTime, unitEffects);
                }
            }
        }

        public void ManageLifeEffect(Effect<Unit> effect, float deltaTime, KeyValuePair<Unit, (List<Effect<Unit>> attackEffects, List<Effect<Unit>> moveEffect)> unitEffects)
        {
            if (effect.CheckApply(unitEffects.Key))
            {
                effect.ApplyEffect(unitEffects.Key, effect.Modifier, deltaTime);
            }
            effect.EffectDuration -= deltaTime;
            if (effect.EffectDuration <= 0)
            {
                RemoveItem(unitEffects.Key, effect);
            }
        }
        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(UpdateEffects);
        }
    }
}
