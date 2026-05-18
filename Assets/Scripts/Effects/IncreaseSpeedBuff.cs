namespace Effects
{
    public class IncreaseSpeedBuff : Buff
    {
    	public override float moveDelayModifier => modifier;
    	public override float attackDelayModifier => 1f;
    }
}