using Model.Config;
using UnityEngine;

namespace Model.Runtime
{
    public sealed class ThirdUnit : Unit
    {
        public ThirdUnit(UnitConfig config, Vector2Int startPos)
            : base(config, startPos)
        {
        }
    }
}