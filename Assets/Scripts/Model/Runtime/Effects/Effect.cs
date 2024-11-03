namespace Assets.Scripts.Model.Runtime.Effects
{
    public abstract class Effect
    {
        public abstract float Duration { get; set; }
        public abstract float MoveDelayModifier { get; }
        public abstract float AttackDelayModifier { get; }
    }
}