using Model.Runtime;
using System.Collections.Generic;
using Utilities;

public class BuffController
{ 
    internal void addEffect(Unit unit, AbstractBuff effect)
    {
        switch (unit.Config.UnitType)
        {
            case 2:
                unit.Config._doubleShot = true;
                break;
            case 3:
                unit.Config._attackRangeBuff = 3;
                break;
            default:
                break;
        }
    }
}