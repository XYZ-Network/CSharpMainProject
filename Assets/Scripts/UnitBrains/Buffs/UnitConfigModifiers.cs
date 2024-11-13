using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class UnitConfigModifiers
    {
        public float MoveDelay { get; set; } = 0;
        public float AttackDelay { get; set; } = 0;
        public float AttackRange { get; set; } = 0;
        public int MaxTargets { get; set; } = 0;
    }
}
