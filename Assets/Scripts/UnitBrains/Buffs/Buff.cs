using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public abstract class Buff<T> : BaseBuff where T : BaseUnitBrain
    {

        public Buff(string id, float modifier, float timeRemains) : base(id, modifier, timeRemains)
        {
        }

        public override  bool CanBeAppliedTo(IReadOnlyUnit unit) {
            return unit.UnitBrain.GetType().Equals(typeof(T));
        }

        public override void ReduceTimeRemains(float amount) {
            TimeRemains = Math.Max(0f, TimeRemains - amount);
        }

        public override bool Equals(object obj)
        {
            return obj is Buff<T> buff &&
                   Id == buff.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
