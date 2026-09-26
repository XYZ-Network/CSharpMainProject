namespace Model.Runtime.Buffs
{
    public interface IBuff
    {
        string Id { get; }
        bool IsExpired { get; }

        bool CanApply(Unit unit);
        void Apply(Unit unit);
        void Remove(Unit unit);
        void Update(float deltaTime);
    }
}