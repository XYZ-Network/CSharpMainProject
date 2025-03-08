using System;
using System.Collections.Generic;
using Model.Runtime;
using UnityEngine;

namespace UnitBrains.BuffSystem
{
    public abstract class Buff<T>
    {
        public abstract string Name { get; }
        public float Duration { get; protected set; }
        public bool IsExpired => Duration <= 0;
        
        protected List<T> _targetUnitList = new List<T>();
        protected List<T> _buffsToRemove = new List<T>();
        
        public Buff(float duration)
        {
            Duration = duration;
        }
        
        public abstract void ApplyBuff(T unit);
        public abstract void RemoveBuff();
        public abstract bool CanApply(T unit);
        
        public void Update(float deltaTime)
        {
            if(_targetUnitList.Count > 0)
            if (Duration > 0)
            {
                Duration -= deltaTime;
            }
            
            if(IsExpired) RemoveBuff();
        }
    }
}