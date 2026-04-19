
using Model.Runtime;

public class SpeedBuff : Buff<Unit>
{
    private float _multiplier;

    public SpeedBuff(float duration, float multiplier) : base(duration)
    {
        _multiplier = multiplier;
    }

    public override void Apply(Unit target)
    {
        target.ModifyMoveSpeed(_multiplier);
    }

    public override void Remove(Unit target)
    {
        target.ModifyMoveSpeed(1f / _multiplier);
    }
}