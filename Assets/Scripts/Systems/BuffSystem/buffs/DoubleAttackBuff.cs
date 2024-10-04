using System.Diagnostics;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnityEngine;

public class DoubleAttackBuff : AbstractBuff
{
    public override BuffNames Type => BuffNames.DoubleAttackBuff;

    public override float Modifier => 2f;
    private BaseUnitBrain _unitBrain;

    public DoubleAttackBuff(IReadOnlyUnit unit) : base(unit)
    {
        _unitBrain = unit.getBrain();
    }


    public override void Apply()
    {
        if (isAppliesFor(this.unit)) _unitBrain.setProjectilesModifier((int)Modifier);
    }

    public override bool isAppliesFor(IReadOnlyUnit unit)
    {
        return _unitBrain.TargetUnitName == "Cobra Commando";
    }

    public override void Dispose()
    {
        _unitBrain.setProjectilesModifier();
    }
}