using System;
namespace AssemblyCSharp.Assets.Scripts.Controller
{
    public interface IBuff<T> 
    {
        bool CanApply(T unit);
        void Apply(T unit);
    }
}
