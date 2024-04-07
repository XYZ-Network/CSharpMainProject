using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Enemy
{
    public class DefaultEnemyUnitBrain : BaseUnitBrain
    {
        public override bool IsPlayerUnitBrain => false;
    }
}