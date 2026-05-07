using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    public enum BuffType
    {
        AttackSpeed,
        MoveSpeed,
        AttackRange,
        AttackCount
    }

    public abstract class Buff<T> where T : IModifiableUnit
    {
        public BuffType Type { get; }
        public float Duration { get; private set; }
        public float Modifier { get; }
        protected T TargetUnit { get; private set; }

        private float _elapsedTime;

        protected Buff(BuffType type, float duration, float modifier)
        {
            Type = type;
            Duration = duration;
            Modifier = modifier;
            _elapsedTime = 0f;
        }

        // Свойство для проверки истечения срока действия
        public bool IsExpired => _elapsedTime >= Duration;

        public virtual bool CanApply(T unit) => true;

        public void Apply(T unit)
        {
            TargetUnit = unit;
            OnApply();
        }

        public void Update(float deltaTime)
        {
            _elapsedTime += deltaTime;
            OnUpdate(deltaTime);

            if (IsExpired)
            {
                OnRemove();
            }
        }

        public void Remove()
        {
            OnRemove();
        }

        protected abstract void OnApply();
        protected abstract void OnUpdate(float deltaTime);
        protected abstract void OnRemove();
    }
}
