namespace Effects
{
    public class DecreaseAttackSpeedBuff : Buff
    {
        public override float moveDelayModifier => 1f;
    	public override float attackDelayModifier => 1f / modifier;
    }
}