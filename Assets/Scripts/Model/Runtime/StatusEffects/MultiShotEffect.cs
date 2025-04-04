namespace Model.Runtime.StatusEffects
{
    public class MultiShotEffect : TemplateStatusEffect<IStatsDynamic>
    {
        private const float _defaultModifier = 1f;
        private const float _defaultDuration = 0.7f;

        public MultiShotEffect(float duration = _defaultDuration, float modifier = _defaultModifier)
        {
            base.Type = StatusEffectType.MultiShot;
            base.Duration = duration;
            base.Modifier = modifier;
        }

        public override bool CanApply(IStatsDynamic unit)
        {
            if (unit.GetName() != "Cobra Commando")
                return false;

            return true;
        }

        public override void EndEffect(IStatsDynamic unit)
        {
            unit.ChangeMultiplierShot(-(int)base.Modifier);
        }

        public override void StartEffect(IStatsDynamic unit)
        {
            unit.ChangeMultiplierShot(+(int)base.Modifier);
        }
    }
}
