using UnityEngine;

namespace StatusEffects
{
    public enum Attributes
    {
        AttackInterval,
        MovementInterval,
    }
    
    public abstract class StatusEffect
    {
        public Attributes Attribute => _attribute;
        public float Modifier => _modifier;
        public float Duration => _duration;
        public float ActivationTime => _activationTime;
        public bool IsActive => _isActive;
        public bool IsBuff { get; protected set; }

        private readonly Attributes _attribute;
        private readonly float _modifier;
        private readonly float _duration;
        private float _activationTime = 0f;
        private bool _isActive;
        
        public StatusEffect(Attributes attribute, float modifier, float duration)
        {
            _attribute = attribute;
            _modifier = modifier;
            _duration = duration;
        }

        
        public void Activate()
        {
            _isActive = true;
            _activationTime = Time.time;
        }

        
        public void Deactivate()
        {
            _isActive = false;
            _activationTime = 0f;
        }

        
        public override bool Equals(object obj)
        {
            if (obj is not StatusEffect effect)
                return false;

            return _attribute == effect.Attribute;
        }

        
        public override int GetHashCode()
        {
            return (int)_attribute;
        }
    }
}