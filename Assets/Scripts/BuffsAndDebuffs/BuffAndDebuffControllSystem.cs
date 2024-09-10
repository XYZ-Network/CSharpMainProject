using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Model.Runtime;
using Model.Runtime.ReadOnly;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class BuffAndDebuffControllSystem
    {
        private TimeUtil _timeUtil;
        private Dictionary<Unit, (List<Effect> attackEffects, List<Effect> moveEffects)> _effectsDictionary = new Dictionary<Unit, (List<Effect> attackEffects, List<Effect> moveEffect)>();

        public BuffAndDebuffControllSystem()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(updateEffects);
        }
        public void AddItem(Unit unit, Effect effect)
        {
            if (!_effectsDictionary.ContainsKey(unit))
            {
                (List<Effect> attackEffects, List<Effect> moveEffect) newTupple = (new List<Effect>(), new List<Effect>());
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

        public void RemoveItem(Unit unit, Effect effect)
        {
            if (_effectsDictionary.ContainsKey(unit))
            {
                switch (effect.EffectType)
                {
                    case EffectType.Attack:
                        _effectsDictionary[unit].attackEffects.Remove(effect);
                        break;
                    case EffectType.Move:
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

        public void updateEffects(float deLtaTime)
        {
            foreach (var unitEffects in _effectsDictionary)
            {
                foreach (Effect effect in unitEffects.Value.attackEffects.ToArray())
                {
                    effect.EffectDuration -= deLtaTime;
                    if (effect.EffectDuration <= 0)
                        RemoveItem(unitEffects.Key, effect);
                }
                foreach (Effect effect in unitEffects.Value.moveEffects.ToArray())
                {
                    effect.EffectDuration -= deLtaTime;
                    if (effect.EffectDuration <= 0)
                        RemoveItem(unitEffects.Key, effect);
                }
            }
        }
        public void Dispose()
        {
            _timeUtil.RemoveFixedUpdateAction(updateEffects);
        }
    }
}
