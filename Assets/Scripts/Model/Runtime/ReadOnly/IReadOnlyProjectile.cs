using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyProjectile
    {
        Vector2 Position { get; }
        float Height { get; }
    }
}