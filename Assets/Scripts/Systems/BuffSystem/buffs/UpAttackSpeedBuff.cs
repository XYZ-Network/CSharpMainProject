using Model.Runtime.ReadOnly;

public class UpAttackSpeedBuff : DownAttackSpeedBuff
{
    public override BuffNames Type => BuffNames.UpAttackSpeed;

    public override float Modifier => 0.2f;

    public UpAttackSpeedBuff(IReadOnlyUnit unit) : base(unit)
    { }
}