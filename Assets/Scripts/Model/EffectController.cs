using Codice.Client.Common;
using Model.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;
using static Model.EffectController;

namespace Model
{
    public class EffectController : MonoBehaviour
    {
        public enum TypesOfEffects
        {
            DelayForNextMoveTimeEffect,
            DelayForNextAttackTimeEffect
        }
        private void OnDestroy()
        {
            UpdateIvent -= UpdateIvent;
        }
        private void Update()
        {
            UpdateIvent?.Invoke();
        }

        public delegate void DelegatForUpdate();
        public DelegatForUpdate UpdateIvent;
        
        private Dictionary<Unit, UnitEffects> DictionaryWithUnitsEffects = new Dictionary<Unit, UnitEffects>() { };
        public void AddEffect(Unit unit,TypesOfEffects typesOfEffects, float effectMultiplier = 0.5f, float time = 1f)
        {
            if (unit != null) 
            {
                if (DictionaryWithUnitsEffects.ContainsKey(unit))
                {
                    DictionaryWithUnitsEffects[unit].AddEffect(unit, time, typesOfEffects, effectMultiplier);
                }
                else
                {
                    DictionaryWithUnitsEffects.Add(unit,new UnitEffects(Enum.GetValues(typeof(TypesOfEffects)).Length));
                    DictionaryWithUnitsEffects[unit].AddEffect(unit, time, typesOfEffects, effectMultiplier);
                }
            }
        }
        public float FindEffectMultiplier(Unit unit, TypesOfEffects typesOfEffects)
        {
            if (DictionaryWithUnitsEffects.ContainsKey(unit))
                return DictionaryWithUnitsEffects[unit].FindMultiplierEffectOnUnit(typesOfEffects);
            return 1f;
        }
    }
    public class UnitEffects
    {
        float[] UnitEffectsArray;
        Action UpdateIvent;

        public UnitEffects(int NumberOfEffects) 
        {
            UnitEffectsArray = new float[NumberOfEffects];
            for (var i = 0; i < NumberOfEffects; i++)
            {
                UnitEffectsArray[i] = 1f;
            }
        }
        public float FindMultiplierEffectOnUnit(TypesOfEffects typesOfEffects)
        {
            return UnitEffectsArray[(int)typesOfEffects];
        }
        public void AddEffect(Unit unit, float time, TypesOfEffects typesOfEffects, float effectMultiplier)
        {
            new EffectsBehavior(unit, time, UnitEffectsArray, typesOfEffects, effectMultiplier);
        }
    }
    public class EffectsBehavior
    { 
        public bool EffectIsFinish;
        float CD;
        float EffectMultiplier;
        float ActivationTime;
        float[] EffectsArr;
        TypesOfEffects EffectsEnum;
        Unit unit;

        public EffectsBehavior(Unit unit, float time,float[] EffectsArr, TypesOfEffects effectsEnum, float effectMultiplier) 
        {
            EffectIsFinish = false ;
            this.unit = unit;
            CD = time;
            EffectMultiplier = effectMultiplier;
            this.EffectsArr = EffectsArr;
            ActivationTime = UnityEngine.Time.time;
            EffectsArr[(int)effectsEnum] *= EffectMultiplier;
            EffectsEnum = effectsEnum;
            ServiceLocator.Get<EffectController>().UpdateIvent += EffectDC;
        }
        ~EffectsBehavior() 
        {
            FinishEffect();
        }
        public void FinishEffect() 
        {
            ServiceLocator.Get<EffectController>().UpdateIvent -= EffectDC;
            if (!EffectIsFinish)
            {
                if (EffectsArr[(int)EffectsEnum] != 0)
                    EffectsArr[(int)EffectsEnum] /= EffectMultiplier;
                else
                    EffectsArr[(int)EffectsEnum] = 0;
            }
            EffectIsFinish = true;
        }
        void EffectDC() 
        {
            if (UnityEngine.Time.time > ActivationTime + CD || unit.IsDead)
            {
                FinishEffect();
            }
        }
    }
}
