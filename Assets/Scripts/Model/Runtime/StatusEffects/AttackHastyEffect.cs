namespace Model.Runtime.StatusEffects
{
    public class AttackHastyEffect : TemplateStatusEffect<IStatsDynamic>
    {
        private const float _defaultModifier = 0.35f;
        private const float _defaultDuration = 0.7f;

        public AttackHastyEffect(float duration = _defaultDuration, float modifier = _defaultModifier)
        {
            base.Type = StatusEffectType.AttackHasty;
            base.Duration = duration;
            base.Modifier = modifier;
        }

        public override bool CanApply(IStatsDynamic unit)
        {
            if (unit.GetName() != "Sky Serpent")
                return false;

            return true;
        }

        public override void EndEffect(IStatsDynamic unit)
        {
            unit.ChangeDelayAttack(+base.Modifier);
        }

        public override void StartEffect(IStatsDynamic unit)
        {
            unit.ChangeDelayAttack(-base.Modifier);
        }
    }
}
