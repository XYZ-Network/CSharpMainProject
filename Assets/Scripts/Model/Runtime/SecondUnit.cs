using Model.Config;
using UnityEngine;

namespace Model.Runtime
{
    public sealed class SecondUnit : Unit
    {
        public SecondUnit(UnitConfig config, Vector2Int startPos)
            : base(config, startPos)
        {
        }
    }
}