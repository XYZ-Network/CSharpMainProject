using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using StatusEffects;
using StatusEffects.ScriptableObjects;
using UnityEngine;
using Utilities;
using View;
using Random = System.Random;

namespace Controller
{
    public enum EffectType
    {
        Buff,
        Debuff,
    }
    
    public class StatusEffectsController : IDisposable
    {
        public Dictionary<IReadOnlyUnit, HashSet<StatusEffect>> AllUnitsEffects => _allUnitsEffects;
        public Action<Vector2Int, VFXView.VFXType> BuffApplied;

        private readonly Dictionary<IReadOnlyUnit, HashSet<StatusEffect>> _allUnitsEffects;
        private readonly StatusEffectsData _statusEffectsData;
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly LevelController _levelController;
        private readonly TimeUtil _timeUtil;
        private readonly int _maxEffectsOnUnit = 2;
        private Random _random = new();

        public StatusEffectsController(IReadOnlyRuntimeModel runtimeModel, LevelController levelController)
        {
            _runtimeModel = runtimeModel;
            _levelController = levelController;
            _statusEffectsData = Resources.Load<StatusEffectsData>("GameData/StatusEffectsData");
            _allUnitsEffects = new();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            
            _levelController.SimulationStarted += OnSimulationStarted;
            _timeUtil.AddUpdateAction(RemoveExpiredEffects);
        }


        public float GetAttributeModifier(IReadOnlyUnit unit, Attributes attribute, float configValue)
        {
            var effect = _allUnitsEffects[unit].FirstOrDefault(e => e.Attribute == attribute);
            
            if (effect == null || !effect.IsActive)
                return configValue;

            if (effect.Modifier < 1f)
                return effect.IsBuff ? (configValue * (1f - effect.Modifier)) : (configValue * (1f + effect.Modifier));

            return effect.IsBuff ? (configValue / effect.Modifier) : (configValue * effect.Modifier);
        }


        private void OnSimulationStarted()
        {
            _allUnitsEffects.Clear();
            
            var allUnits = _runtimeModel.RoUnits;

            foreach (var unit in allUnits)
            {
                _allUnitsEffects.Add(unit, new HashSet<StatusEffect>());
            }
        }


        public void ApplyEffectOnUnit(IReadOnlyUnit unit, EffectType type)
        {
            if (_allUnitsEffects[unit].Count < _maxEffectsOnUnit)
            {
                var randomEffect = GetEffect(type);
                randomEffect.Activate();
                BuffApplied?.Invoke(unit.Pos,VFXView.VFXType.BuffApplied);
                _allUnitsEffects[unit].Add(randomEffect);
            }
        }
        
        
        private StatusEffect GetEffect(EffectType type)
        {
            var effects = type is EffectType.Buff ? _statusEffectsData.Buffs : _statusEffectsData.Debuffs;
            var effectIndex = _random.Next(0, effects.Count());
            var result = effects.ElementAt(effectIndex);

            if (result.IsBuff)
                return new Buff(result.Attribute, result.Modifier, result.Duration);

            return new Debuff(result.Attribute, result.Modifier, result.Duration);
        }
        
        
        private void RemoveExpiredEffects(float deltaTime)
        {
            foreach (var originalSet in _allUnitsEffects.Values)
            {
                var copiedSet = new HashSet<StatusEffect>(originalSet);
                
                foreach (var effect in copiedSet)
                {
                    if (Time.time - effect.ActivationTime >= effect.Duration)
                        originalSet.Remove(effect);
                }
            }
        }

        
        public void Dispose()
        {
            _levelController.SimulationStarted -= OnSimulationStarted;
            _timeUtil.RemoveUpdateAction(RemoveExpiredEffects);
        }
    }
    
}