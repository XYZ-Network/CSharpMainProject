using System.Diagnostics;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnityEngine;

public class UpRangeBuff : AbstractBuff
{
    public override BuffNames Type => BuffNames.UpRangeBuff;

    public override float Modifier => 3f;
    private BaseUnitBrain _unitBrain;

    public UpRangeBuff(IReadOnlyUnit unit) : base(unit)
    {
        _unitBrain = unit.getBrain();
    }


    public override void Apply()
    {
        if (isAppliesFor(this.unit)) _unitBrain.setRangeModifier((int)Modifier);
    }

    public override bool isAppliesFor(IReadOnlyUnit unit)
    {
        return _unitBrain.TargetUnitName == "Ironclad Behemoth";
    }

    public override void Dispose()
    {
        _unitBrain.setRangeModifier();
    }
}