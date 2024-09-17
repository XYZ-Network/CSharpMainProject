using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime.ReadOnly;
using UnitBrains;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public enum EffectType
    {
        Attack,
        Move,
        DAttack,
        DMove
    }
    public abstract class Effect <T> where T : Unit

    {
        private T _effectOwner;
        private float _effectDuration;
        private float _modifier;
        private EffectType _effectType;

        public float EffectDuration { get { return _effectDuration; } set { _effectDuration = value; } }
        public float Modifier { get { return _modifier; } set { _modifier = value; } }
        public EffectType EffectType { get { return _effectType; } }
        public Effect(T unit, EffectType effectType)
        {
            _effectOwner = unit;
            _effectType = effectType;
        }

        public virtual bool CheckApply (T _owner)
        { 
            return true;
        }

        public virtual void ApplyEffect(T _owner, float modifier, float time)
        {

        }

        public virtual void ClearEffect(T _owner)
        {
        }
    }
}