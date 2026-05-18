namespace Effects
{
    public abstract class Buff
    {
        public float duration;
        public float modifier;

    	public abstract float moveDelayModifier { get; }
    	public abstract float attackDelayModifier { get; }
	}
}