using Model.Runtime.ReadOnly;

public class DownMoveSpeedBuff : AbstractBuff
{
    public override BuffNames Type => BuffNames.DownMoveSpeed;

    public override float Modifier => 1.5f;

    public DownMoveSpeedBuff(IReadOnlyUnit unit) : base(unit)
    { }

    public override void Apply()
    {
        this.unit.setMoveModifier(Modifier);
    }

    public override void Dispose()
    {
        this.unit.setMoveModifier();
    }
}