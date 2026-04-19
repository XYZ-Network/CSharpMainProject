
using Model.Runtime;

public class AttackRateBuff : Buff<Unit>
{
    private float _multiplier;

    public AttackRateBuff(float duration, float multiplier) : base(duration)
    {
        _multiplier = multiplier;
    }

    public override void Apply(Unit target)
    {
        target.ModifyAttackSpeed(_multiplier);
    }

    public override void Remove(Unit target)
    {
        target.ModifyAttackSpeed(1f / _multiplier);
    }
}
