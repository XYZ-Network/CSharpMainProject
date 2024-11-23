using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Assets.Scripts.BuffsAndDebuffs
{
    public class AttackRadiusBuff<T> : Effect<T> where T : Unit
    {
        public AttackRadiusBuff(T _unit) : base(_unit, EffectType.Attack)
        {
            Modifier = 2f;
            EffectDuration = 30f;
        }

        public override bool CheckApply(T _owner)
        {

            return _owner.Config.Name == "Ironclad Behemoth" ? base.CheckApply(_owner) : false;
        }

        public override void ApplyEffect(T _owner, float modifier, float time)
        {
            _owner.SetRangeModifier(Modifier);
        }

        public override void ClearEffect(T _owner)
        {
            _owner.SetRangeModifier(1.0f);
        }
    }

}
