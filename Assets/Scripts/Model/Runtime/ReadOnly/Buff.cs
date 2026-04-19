using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Model.Runtime
{
    public abstract class Buff<T> : IBuff where T : Unit
    {
        public float Duration {  get; protected set; }

        public float TimeLeft { get;  set; }

        protected Buff(float duration)
        {
            Duration = duration;
            TimeLeft = duration;
        }

        public abstract void Apply(T target);


        public abstract void Remove(T target);

        void IBuff.Apply(Unit target)
        {
            if (target is T typedTarget)
            {
                Apply(typedTarget);
            }
        }

        void IBuff.Remove(Unit target) 
        { 
        if(target is T typedTarget)
            {
                Remove(typedTarget);
            }
        }
       
    }
}
