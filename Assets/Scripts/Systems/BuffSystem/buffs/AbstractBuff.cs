using Model.Runtime;
using Model.Runtime.ReadOnly;

public abstract class AbstractBuff
{
    public abstract BuffNames Type { get; }
    public virtual float Modifier { get; }
    public float Duration = 6f;

    public IReadOnlyUnit unit { get; }

    public AbstractBuff(IReadOnlyUnit unit) {
        this.unit = unit;
    }

    public abstract void Apply();

    public abstract void Dispose();

    public virtual bool isAppliesFor(IReadOnlyUnit unit)
    {
        return true;
    }
}