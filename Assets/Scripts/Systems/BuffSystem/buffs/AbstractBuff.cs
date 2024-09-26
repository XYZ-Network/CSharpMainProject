using Model.Runtime;

public abstract class AbstractBuff
{
    public abstract BuffNames Type { get; }
    public abstract float Modifier { get; }
    public float Duration = 6f;
}