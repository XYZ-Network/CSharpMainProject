using System;
using AssemblyCSharp.Assets.Scripts.Controller;
using UnitBrains.Player;
using Unity.VisualScripting;

namespace AssemblyCSharp.Assets.Scripts
{
    public class DoubleShotBuff : IBuff<SecondUnitBrain>
    {
        public bool CanApply(SecondUnitBrain unit)
        {
            return unit.GetOverheatTemperature() < 3f;
        }

        public void Apply(SecondUnitBrain unit)
        {

            unit.IsDoubleShotActive = true;
        }

        public void Remove(SecondUnitBrain unit)
        {

            unit.IsDoubleShotActive = false;
        }

    }
}


