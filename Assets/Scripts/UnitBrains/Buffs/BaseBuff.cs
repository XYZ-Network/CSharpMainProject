using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public abstract class BaseBuff
    {

        public BaseBuff(string id, float modifier, float timeRemains)
        {
            Id = id;
            Modifier = modifier;
            TimeRemains = timeRemains;
        }
        public string Id { get; }
        public abstract bool CanBeAppliedTo(IReadOnlyUnit unit);
        public abstract void Apply(IReadOnlyUnit unit);
        public abstract void Remove(IReadOnlyUnit unit);
        public abstract void ReduceTimeRemains(float amount);
        public float Modifier { get; }
        public float TimeRemains { get; protected set; }
        public bool IsTimeExpired => TimeRemains == 0f;
    }
}
