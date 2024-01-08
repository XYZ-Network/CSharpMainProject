using UnityEngine;

namespace Model.Runtime.ReadOnly
{
    public interface IReadOnlyProjectile
    {
        Vector2 Pos { get; }
        float Height { get; }
    }
}