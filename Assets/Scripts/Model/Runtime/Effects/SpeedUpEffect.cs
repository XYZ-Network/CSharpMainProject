namespace Assets.Scripts.Model.Runtime.Effects
{
    public class SpeedUpEffect : Effect
    {
        public override float Duration { get; set; } = 3f;

        public override float MoveDelayModifier => 0.7f;

        public override float AttackDelayModifier => 1;
    }
}