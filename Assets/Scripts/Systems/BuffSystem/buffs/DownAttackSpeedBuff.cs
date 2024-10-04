using Model.Runtime.ReadOnly;

public class DownAttackSpeedBuff : AbstractBuff
{
    public override BuffNames Type => BuffNames.DownAttackSpeed;

    public override float Modifier => 1.5f;

    public DownAttackSpeedBuff(IReadOnlyUnit unit) : base(unit)
    { }

    public override void Apply()
    {
        this.unit.setAttackModifier(Modifier);
    }

    public override void Dispose() {
        this.unit.setAttackModifier();
    }
}