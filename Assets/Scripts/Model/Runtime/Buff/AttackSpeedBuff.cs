using Assets.Scripts.Model.Runtime;
using Model.Runtime;
using Model.Runtime.ReadOnly;

public class AttackSpeedBuff : AbstractBuff
{
    public AttackSpeedBuff() : base(3, 2, BuffType.AttackSpeed)
    {
    }

    public override void ApplyBuff(IWriteUnit unit)
    {
        ClearBuff(unit);
        unit.AttackDelay = unit.AttackDelay + Modifier;
    }

    public override void ClearBuff(IWriteUnit unit)
    {
        unit.ResetAttackDelayToDefault();
    }
}
