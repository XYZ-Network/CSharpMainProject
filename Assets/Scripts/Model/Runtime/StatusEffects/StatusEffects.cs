using System;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utilities;

namespace Model.Runtime.StatusEffects
{
    public class StatusEffects
    {
        private RuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;
        private System.Random _random;

        private const float DefaultDuration = 0.7f;
        private const float DefaultModifeir = 0.25f;

        public event Action<int> NewStatusEffect;
        public event Action<int> EndStatusEffect;

        public StatusEffects(RuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;

            _timeUtil = timeUtil;

            _random = new System.Random();
        }

        public void StartStatusEffectProcessing()
        {
            _timeUtil.AddFixedUpdateAction(TickingStatusEffects);
            //_timeUtil.AddFixedUpdateAction(GeneratorRandomStatusEffects);
        }
        public void StopStatusEffectProcessing()
        {
            _timeUtil.RemoveFixedUpdateAction(TickingStatusEffects);
            //_timeUtil.RemoveFixedUpdateAction(GeneratorRandomStatusEffects);
        }

        private StatusEffectType GetRandomStatusEffectType()
        {
            Array enumValues = Enum.GetValues(typeof(StatusEffectType));

            return
                (StatusEffectType)enumValues.GetValue(_random.Next(enumValues.Length));
        }

        public void AddRandomStatusEffect(int unitID)
        {
            AddStatusEffect(unitID, GetRandomStatusEffectType());
        }

        public void AddStatusEffect(int unitID, StatusEffectType effectType)
        {
            if (!_runtimeModel.UnitStatusEffects.ContainsKey(unitID))
            {
                HashSet<BaseStatusEffect> newEffects = new HashSet<BaseStatusEffect>();
                _runtimeModel.UnitStatusEffects.Add(unitID, newEffects);
            }

            if (_runtimeModel.UnitStatusEffects[unitID].Count == 0)
            {
                BaseStatusEffect effect
                    = new BaseStatusEffect(effectType, DefaultDuration, DefaultModifeir);

                bool success_flg = false;
                success_flg = _runtimeModel.UnitStatusEffects[unitID].Add(effect);

                if (success_flg)
                {
                    NewStatusEffect?.Invoke(unitID);
                    Debug.Log($"Add effect UnitID = {unitID}");
                }
                else
                {
                    Debug.Log($"Cannot add effect UnitID = {unitID}");
                }
            }
        }

        public void GeneratorRandomStatusEffects(float deltaTime)
        {
            var allUnits = _runtimeModel.RoUnits;

            foreach (var unit in allUnits)
            {
                AddRandomStatusEffect(unit.UnitId);
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

                int success_cnt = 0;
                success_cnt = unitEffects.Value.RemoveWhere(TimeIsOver);
                if (success_cnt>0)
                {
                    EndStatusEffect?.Invoke(unitEffects.Key);
                    Debug.Log($"End effect UnitID = {unitEffects.Key}");
                }
            }
        }

        public bool HasStatusEffect(int unitID)
        {
            if (!_runtimeModel.UnitStatusEffects.ContainsKey(unitID))
                return false;

            if (_runtimeModel.UnitStatusEffects[unitID].Count > 0)
                return true;

            return false;
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

