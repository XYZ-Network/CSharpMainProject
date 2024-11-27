using Model.Runtime;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using View;

namespace Model.Effects
{
    public class _EffectController : MonoBehaviour
    {
        
        private Dictionary<Unit, UnitEffects> DictionaryWithUnitsEffects = new Dictionary<Unit, UnitEffects>() { };
        public void AddEffect(Unit unit, BaseEffect<Unit> typesOfEffects, float effectMultiplier = 0.5f, float time = 1f)
        {
            if (unit != null && typesOfEffects.AbleAddEffect(unit)) 
            {
                ServiceLocator.Get<VFXView>()?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);

                if (DictionaryWithUnitsEffects.ContainsKey(unit))
                {
                    DictionaryWithUnitsEffects[unit].AddEffect(unit, time, typesOfEffects, effectMultiplier);
                }
                else
                {
                    DictionaryWithUnitsEffects.Add(unit, new UnitEffects());
                    DictionaryWithUnitsEffects[unit].AddEffect(unit, time, typesOfEffects, effectMultiplier);
                }
            }
        }

    }
    public class UnitEffects
    {
        List<EffectsBehavior> UnitEffectsArray = new List<EffectsBehavior>();
        public void AddEffect(Unit unit, float time, BaseEffect<Unit> typesOfEffects, float effectMultiplier)
        {
            var flag = true;
            foreach (var i in UnitEffectsArray)
            {
                if (i.GetEffectType.Name == typesOfEffects.Name)
                {
                    flag = false;
                    i.FinishEffect();
                    i.AddEffectIfLastFinish(time, effectMultiplier);
                    break;
                }
            }
            if (flag)
                UnitEffectsArray.Add(new EffectsBehavior(unit, time, typesOfEffects, effectMultiplier));
            
        }
    }

    public class EffectsBehavior
    { 
        public bool EffectIsFinish = true;
        float CD;
        float ActivationTime;
        BaseEffect<Unit> TypesOfEffect;
        Unit unit;
        TimeUtil _timeUtil;
        public EffectsBehavior(Unit unit, float time, BaseEffect<Unit> typesOfEffect, float effectMultiplier) 
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            this.unit = unit;
            TypesOfEffect = typesOfEffect;
            AddEffectIfLastFinish(time, effectMultiplier);
        }
        ~EffectsBehavior() 
        {
            FinishEffect();
        }
        public BaseEffect<Unit> GetEffectType => TypesOfEffect;
        public void FinishEffect() 
        {
            if (!EffectIsFinish)
            {
                TypesOfEffect.ClearEffect(unit);
                _timeUtil.RemoveUpdateAction(EffectDC);
            }
            EffectIsFinish = true;
        }
        public void AddEffectIfLastFinish(float time, float effectMultiplier) 
        {
            if (!EffectIsFinish) return;

            EffectIsFinish = false;
            CD = time;
            ActivationTime = UnityEngine.Time.time;
            _timeUtil.AddUpdateAction(EffectDC);
            TypesOfEffect.AddEffect(unit, effectMultiplier);

        }
        void EffectDC(float time) 
        {
            if (UnityEngine.Time.time > ActivationTime + CD || unit.IsDead)
            {
                FinishEffect();
            }
        }
    }
}
