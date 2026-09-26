namespace Model.Runtime.Buffs
{
    public sealed class SecondUnitDoubleShotBuff : Buff<SecondUnit>
    {
        public SecondUnitDoubleShotBuff(float duration)
            : base(duration, "DoubleShot")
        {
        }

        protected override void OnApply(SecondUnit unit)
        {
            unit.ModifyProjectileCount(2);
        }

        protected override void OnRemove(SecondUnit unit)
        {
            unit.ModifyProjectileCount(1);
        }
    }
}