using System;
using AssemblyCSharp.Assets.Scripts.Controller;

namespace AssemblyCSharp.Assets.Scripts
{
    public abstract class AbsBuff<T> : IBuff<T>
    {
        public abstract bool CanApply(T unit);
        public abstract void Apply(T unit);
    }
}
