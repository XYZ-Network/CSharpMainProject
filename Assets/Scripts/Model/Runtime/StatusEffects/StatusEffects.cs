using System;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utilities;
using static UnityEngine.UI.CanvasScaler;

namespace Model.Runtime.StatusEffects
{
    public class StatusEffects
    {
        private RuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private System.Random _random;

        private const float DefaultDuration = 2f;
        private const float DefaultModifeir = 0.2f;

        public StatusEffects(RuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;

            _timeUtil = timeUtil;

            _random = new System.Random();
        }

        public void StartStatusEffectProcessing()
        {
            _timeUtil.AddFixedUpdateAction(TickingStatusEffects);
            _timeUtil.AddFixedUpdateAction(GeneratorRandomStatusEffects);
        }
        public void StopStatusEffectProcessing()
        {
            _timeUtil.RemoveFixedUpdateAction(TickingStatusEffects);
            _timeUtil.RemoveFixedUpdateAction(GeneratorRandomStatusEffects);
        }

        private StatusEffectType GetRandomStatusEffectType()
        {
            Array enumValues = Enum.GetValues(typeof(StatusEffectType));

            return
                (StatusEffectType)enumValues.GetValue(_random.Next(enumValues.Length));
        }

        public void GeneratorRandomStatusEffects(float deltaTime)
        {
            var allUnits = _runtimeModel.RoUnits;

            foreach (var unit in allUnits)
            {
                BaseStatusEffect effect
                    = new BaseStatusEffect(GetRandomStatusEffectType(), DefaultDuration, DefaultModifeir);

                if (_runtimeModel.UnitStatusEffects.ContainsKey(unit.UnitId))
                {
                    if (_runtimeModel.UnitStatusEffects[unit.UnitId].Count == 0)
                    {
                        _runtimeModel.UnitStatusEffects[unit.UnitId].Add(effect);
                        Debug.Log($"Add effect UnitID = {unit.UnitId}");
                    }
                }
                else
                {
                    HashSet<BaseStatusEffect> newEffects = new HashSet<BaseStatusEffect>();
                    newEffects.Add(effect);
                    _runtimeModel.UnitStatusEffects.Add(unit.UnitId, newEffects);
                    Debug.Log($"Add effect UnitID = {unit.UnitId}");
                }
            }
        }

        private bool TimeIsOver(BaseStatusEffect effect)
        {
            return (effect.Duration <= 0);
        }

        public void TickingStatusEffects(float deltaTime)
        {
            var allStatusEffects = _runtimeModel.UnitStatusEffects;

            foreach (var unitEffects in allStatusEffects)
            {
                foreach (var effect in unitEffects.Value)
                {
                    effect.Duration -= deltaTime;
                }

                unitEffects.Value.RemoveWhere(TimeIsOver);
            }
        }

        public float GetMoveStatusEffectModifeir(int unitID)
        {
            if (!_runtimeModel.UnitStatusEffects.ContainsKey(unitID))
               return 0f;

            var unitStatusEffect = _runtimeModel.UnitStatusEffects[unitID];

            BaseStatusEffect buffMove = new (StatusEffectType.HastyMovement);
            float buff
                = unitStatusEffect.TryGetValue(buffMove, out buffMove) ? buffMove.Modifier : 0f;

            BaseStatusEffect debuffMove = new(StatusEffectType.SlowMovement);
            float debuff
                = unitStatusEffect.TryGetValue(debuffMove, out debuffMove) ? debuffMove.Modifier : 0f;

            return debuff - buff;
        }

        public float GetAttackStatusEffectModifeir(int unitID)
        {
            if (!_runtimeModel.UnitStatusEffects.ContainsKey(unitID))
                return 0f;

            var unitStatusEffect = _runtimeModel.UnitStatusEffects[unitID];

            BaseStatusEffect buffAttack = new(StatusEffectType.HastyAttack);
            float buff =
                unitStatusEffect.TryGetValue(buffAttack, out buffAttack) ? buffAttack.Modifier : 0f;

            BaseStatusEffect debuffAttack = new(StatusEffectType.SlowAttack);
            float debuff =
                unitStatusEffect.TryGetValue(debuffAttack, out debuffAttack) ? debuffAttack.Modifier : 0f;

            return debuff - buff;
        }
    }
}

