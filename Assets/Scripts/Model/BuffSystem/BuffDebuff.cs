// Scripts/Model/BuffSystem/BuffDebuff.cs
namespace Model.BuffSystem
{
    public class BuffDebuff
    {
        public BuffType Type { get; }
        public float Modifier { get; }
        public float Duration { get; private set; }
        public bool IsExpired => Duration <= 0f;

        public BuffDebuff(BuffType type, float modifier, float duration)
        {
            Type = type;
            Modifier = modifier;
            Duration = duration;
        }

        public void Update(float deltaTime)
        {
            Duration -= deltaTime;
        }
    }
}
