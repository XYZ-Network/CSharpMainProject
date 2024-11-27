using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Model.Effects._EffectController;

namespace Model.Effects
{
    public abstract class BaseEffect<T> where T : Unit
    {
        protected float _baseValue = -1;
        public abstract string Name { get; }
        public abstract void AddEffect(T unit, float effectMultiplier);
        public abstract void ClearEffect(T unit);
        public virtual bool AbleAddEffect(T unit)
        {
            return true;
        }
    }
}
