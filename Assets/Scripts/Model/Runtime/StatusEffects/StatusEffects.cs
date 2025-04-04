using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Model.Runtime.StatusEffects
{
    public class StatusEffects
    {
        private RuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private const float DefaultDuration = 0.7f;
        private const float DefaultModifier = 0.25f;

        public event Action<int> NewStatusEffect;
        public event Action<int> EndStatusEffect;

        public StatusEffects(RuntimeModel runtimeModel, TimeUtil timeUtil)
        {
            _runtimeModel = runtimeModel;

            _timeUtil = timeUtil;
        }

        public void StartStatusEffectProcessing()
        {
            _timeUtil.AddFixedUpdateAction(TickingStatusEffects);
        }
        public void StopStatusEffectProcessing()
        {
            _timeUtil.RemoveFixedUpdateAction(TickingStatusEffects);
        }

        public bool TryAddStatusEffect(int unitID, TemplateStatusEffect<IStatsDynamic> effect)
        {
            var unit = _runtimeModel.AllUnits.FirstOrDefault(u => u.UnitId == unitID);
            if (unit == null)
            {
                Debug.Log($"Cannot add effect UnitID = {unitID}. Unit not found");
                return false;
            }

            if (!_runtimeModel.ActiveStatusEffects.ContainsKey(unitID))
            {
                HashSet<TemplateStatusEffect<IStatsDynamic>> newEffects
                    = new HashSet<TemplateStatusEffect<IStatsDynamic>>();

                _runtimeModel.ActiveStatusEffects.Add(unitID, newEffects);
            }

            if (effect.CanApply(unit))
                if (_runtimeModel.ActiveStatusEffects[unitID].Add(effect))
                {
                    effect.StartEffect(unit);
                    NewStatusEffect?.Invoke(unitID);
                    Debug.Log($"Add effect UnitID = {unitID} {unit.GetName()}");
                    return true;
                }

            return false;
        }

        public void TickingStatusEffects(float deltaTime)
        {
            var allStatusEffects = _runtimeModel.ActiveStatusEffects;

            foreach (var unitEffects in allStatusEffects)
            {
                int unitID = unitEffects.Key;

                foreach (var effect in unitEffects.Value)
                {
                    effect.UpdateTimer(deltaTime);
                }

                foreach (var effectIsOver in unitEffects.Value.Where(e => e.TimeIsOver))
                {
                    var unit = _runtimeModel.AllUnits.FirstOrDefault(u => u.UnitId == unitID);
                    if (unit != null)
                    {
                        effectIsOver.EndEffect(unit);
                        EndStatusEffect?.Invoke(unitID);
                        Debug.Log($"End effect UnitID = {unitID} {unit.GetName()}");
                    }
                }

                unitEffects.Value.RemoveWhere(e => e.TimeIsOver);
            }
        }

        public bool HasStatusEffect(int unitID)
        {
            if (!_runtimeModel.ActiveStatusEffects.ContainsKey(unitID))
                return false;

            if (_runtimeModel.ActiveStatusEffects[unitID].Count > 0)
                return true;

            return false;
        }
    }
}

