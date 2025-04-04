namespace Model.Runtime.StatusEffects
{
    public class AttackRangeExpandEffect : TemplateStatusEffect<IStatsDynamic>
    {
        private const float _defaultModifier = 2f;
        private const float _defaultDuration = 0.7f;

        public AttackRangeExpandEffect(float duration = _defaultDuration, float modifier = _defaultModifier)
        {
            base.Type = StatusEffectType.AttackRangeExpand;
            base.Duration = duration;
            base.Modifier = modifier;
        }

        public override bool CanApply(IStatsDynamic unit)
        {
            if (unit.GetName() != "Ironclad Behemoth")
                return false;

            return true;
        }

        public override void EndEffect(IStatsDynamic unit)
        {
            unit.ChangeAttackRange(-(int)base.Modifier);
        }

        public override void StartEffect(IStatsDynamic unit)
        {
            unit.ChangeAttackRange(+(int)base.Modifier);
        }

    }
}
