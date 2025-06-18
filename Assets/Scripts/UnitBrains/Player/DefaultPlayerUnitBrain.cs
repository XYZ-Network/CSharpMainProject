using System.Collections.Generic;
using UnityEngine;

namespace UnitBrains.Player
{
    public class DefaultPlayerUnitBrain : BaseUnitBrain
    {
        // Здесь можно переопределять логику для конкретных юнитов

        public override string TargetUnitName => string.Empty; // например, пустое для всех

        public override void Update(float deltaTime, float time)
        {
            // Можно добавить дополнительную логику передвижения/атаки
            base.Update(deltaTime, time);
        }
    }
}
