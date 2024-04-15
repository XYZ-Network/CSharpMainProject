using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.ReadOnly;
using StatusEffects;
using StatusEffects.ScriptableObjects;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace Controller
{
    public class StatusEffectsController : IDisposable
    {
        private readonly Dictionary<IReadOnlyUnit, List<StatusEffect>> _allUnitsEffects;
        private readonly StatusEffectsData _statusEffectsData;
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly LevelController _levelController;
        private readonly TimeUtil _timeUtil;
        private readonly int _maxEffectsOnUnit = 2;

        public StatusEffectsController(IReadOnlyRuntimeModel runtimeModel, LevelController levelController)
        {
            _runtimeModel = runtimeModel;
            _levelController = levelController;
            _statusEffectsData = Resources.Load<StatusEffectsData>("GameData/StatusEffectsData");
            _allUnitsEffects = new();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            
            _levelController.SimulationStarted += OnSimulationStarted;
            _timeUtil.AddUpdateAction(DeactivateExpiredEffects);
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
                var listOfEffects = GetUnitStatusEffects(_statusEffectsData, _maxEffectsOnUnit);
                _allUnitsEffects.Add(unit, listOfEffects);

                foreach (var effect in listOfEffects)
                {
                    effect.Activate();
                }
            }
        }


        private List<StatusEffect> GetUnitStatusEffects(StatusEffectsData data, int maxEffects)
        {
            HashSet<StatusEffect> unitStatusEffects = new();
            Random random = new();
            
            var buffs = data.Buffs;
            var debuffs = data.Debuffs;

            for (int i = 0; i < maxEffects; i++)
            {
                if (random.Next(0, 2) == 1)
                    unitStatusEffects.Add(GetRandomEffect(buffs));
                else
                    unitStatusEffects.Add(GetRandomEffect(debuffs));
            }
            
            return unitStatusEffects.ToList();
        }
        
        
        private StatusEffect GetRandomEffect(IEnumerable<EffectInfo> effectInfos)
        {
            Random random = new();
            var effectIndex = random.Next(0, effectInfos.Count());
            var effect = effectInfos.ElementAt(effectIndex);

            if (effect.IsBuff)
                return new Buff(effect.Attribute, effect.Modifier, effect.Duration);

            return new Debuff(effect.Attribute, effect.Modifier, effect.Duration);
        }
        
        
        private void DeactivateExpiredEffects(float deltaTime)
        {
            foreach (var list in _allUnitsEffects.Values)
            {
                foreach (var effect in list)
                {
                    if (!effect.IsActive)
                        continue;

                    if (Time.time - effect.ActivationTime > effect.Duration)
                        effect.Deactivate();
                }
            }
        }

        
        public void Dispose()
        {
            _levelController.SimulationStarted -= OnSimulationStarted;
            _timeUtil.RemoveUpdateAction(DeactivateExpiredEffects);
        }
    }
    
}