using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.Buffs
{
    public class Buff
    {
        public enum BuffType
        {
            AttackSpeed,
            MoveSpeed
        }
        public string Id { get; }
        public BuffType Type { get; }
        public float Modifier { get; }
        public float TimeRemains { get; private set; }
        public bool IsTimeExpired => TimeRemains == 0f;

        public Buff(string id, BuffType type, float modifier, float timeRemains)
        {
            Id = id;
            Type = type;
            Modifier = modifier;
            TimeRemains = timeRemains;
        }

        public void ReduceTimeRemains(float amount)
        {
            TimeRemains = Math.Max(0f, TimeRemains - amount);
        }

        public override bool Equals(object obj)
        {
            return obj is Buff buff &&
                   Id == buff.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}