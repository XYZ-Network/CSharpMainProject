namespace StatusEffects
{
    public class Buff : StatusEffect
    {
        public Buff(Attributes attribute, float modifier, float duration) : base(attribute, modifier, duration)
        {
            IsBuff = true;
        }
    }
}