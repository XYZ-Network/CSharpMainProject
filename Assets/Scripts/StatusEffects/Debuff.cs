namespace StatusEffects
{
    public class Debuff : StatusEffect
    {
        public Debuff(Attributes attribute, float modifier, float duration) : base(attribute, modifier, duration)
        {
            IsBuff = false;
        }
    }
}