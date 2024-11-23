using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Player;
using static Codice.Client.BaseCommands.Import.Commit;
using static UnityEngine.UI.GridLayoutGroup;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class DoubleShotBuff<T> : Effect<T> where T : Unit
    {
        public DoubleShotBuff(T _unit) : base(_unit, EffectType.Attack)
        {
            Modifier = 1.0f;
            EffectDuration = 30f;
        }

        public override void ApplyEffect(T _owner, float modifier, float time)
        {
            _owner.SetDoubleShootMode(true);
        }

        public override void ClearEffect(T _owner)
        {
            _owner.SetDoubleShootMode(false);
        }
    }
}