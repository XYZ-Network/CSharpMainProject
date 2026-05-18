namespace Effects
{
    public class IncreaseAttackSpeedBuff : Buff
    {
        public override float moveDelayModifier => 1f;
    	public override float attackDelayModifier => modifier;
    }
}