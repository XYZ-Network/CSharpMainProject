namespace Assets.Scripts.Model.Runtime
{
    public interface IUnitModifiable
    {
        void ModifyMoveSpeed(float modifier);
        void ModifyAttackSpeed(float modifier);
        void ModifyAttackRange(float modifier);
        void EnableDoubleShot();
        void DisableDoubleShot();

        float CurrentMoveSpeedModifier { get; }
        float CurrentAttackSpeedModifier { get; }
        float CurrentAttackRangeModifier { get; }
        bool IsDoubleShotEnabled { get; }
    }
}