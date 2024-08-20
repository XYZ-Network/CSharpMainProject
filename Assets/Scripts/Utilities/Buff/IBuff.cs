public interface IBuff<T>
{
    float Duration { get; set; }
    void Add(T unit);
    void Remove(T unit);
    bool CanBeAdd(T unit);
    void UpdateDuration(T unit, float deltaTime)
    {
        Duration -= deltaTime;
        if (Duration <= 0)
        {
            Remove(unit);
        }
    }
}