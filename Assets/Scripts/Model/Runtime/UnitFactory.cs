using Model.Config;
using UnityEngine;

namespace Model.Runtime
{
    public static class UnitFactory
    {
        public static Unit Create(UnitConfig config, Vector2Int startPos)
        {
            switch (config.Name)
            {
                case "Cobra Commando":
                    return new SecondUnit(config, startPos);

                case "Ironclad Behemoth":
                    return new ThirdUnit(config, startPos);

                default:
                    return new Unit(config, startPos);
            }
        }
    }
}