namespace Effects
{
    public class DecreaseSpeedBuff : Buff
    {
        public override float moveDelayModifier => 1f / modifier;
    	public override float attackDelayModifier => 1f;
    }
}