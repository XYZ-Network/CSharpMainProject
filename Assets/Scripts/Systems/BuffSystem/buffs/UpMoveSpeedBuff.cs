using Model.Runtime.ReadOnly;

public class UpMoveSpeedBuff : DownMoveSpeedBuff
{
    public override BuffNames Type => BuffNames.UpMoveSpeed;

    public override float Modifier => 0.2f;

    public UpMoveSpeedBuff(IReadOnlyUnit unit) : base(unit)
    { }
}