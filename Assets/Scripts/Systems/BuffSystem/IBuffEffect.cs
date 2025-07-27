namespace Systems.BuffSystem
{
    public interface IBuffEffect
    {
        void Apply(ModifiableParams target);
        float Duration { get; }
    }

    public interface IConditionalBuff
    {
        bool CanApplyTo(ModifiableParams target);
    }
}