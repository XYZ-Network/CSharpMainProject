public class Buff
{
    public float Duration { get; private set; }
    public float MoveSpeedModifier { get; private set; }
    public float AttackSpeedModifier { get; private set; }

    public Buff(float duration, float moveSpeedModifier, float attackSpeedModifier)
    {
        Duration = duration;
        MoveSpeedModifier = moveSpeedModifier;
        AttackSpeedModifier = attackSpeedModifier;
    }

    // Метод для уменьшения времени действия
    public void ReduceDuration(float time)
    {
        Duration -= time;
        if (Duration <= 0)
        {
            OnExpired();
        }
    }

    protected virtual void OnExpired()
    {
        // Действие при истечении времени баффа
    }
}
