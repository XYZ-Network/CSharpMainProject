using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UnitBrains.System
{
    public enum DebuffType
    {
        SlowAttack,
        SlowMovement,
    }
    public class DebuffEffects : IStatusEffect<DebuffType>
    {
        private DebuffType _name;
        private float _duration = 15f;
        private float _modificator = 1f;
        public DebuffType Name => _name;
        Enum IStatusEffect.Name => Name;
        public float Duration => _duration;
        public float Modificator => _modificator;


        public DebuffEffects(Unit unit)
        {
            int rand = UnityEngine.Random.Range(0, 4);
            BuffType type = (BuffType)rand;
            SetDebuff(rand, unit);
        }
        public DebuffEffects(DebuffType type, Unit unit)
        {
            SetDebuff(type, unit);
        }

        private void SetDebuff<T>(T type, Unit unit)
        {
            switch (type)
            {
                case DebuffType.SlowAttack:
                    _name = DebuffType.SlowAttack;
                    _modificator = 2f;
                    unit.StatModification<float>(Unit.UnitModStats.Attack, _modificator);
                    break;
                case DebuffType.SlowMovement:
                    _name = DebuffType.SlowMovement;
                    _modificator = 10f;
                    unit.StatModification<float>(Unit.UnitModStats.Move, _modificator);
                    break;
            }
        }
        public void BuffBreak(Unit unit)
        {
            switch (_name)
            {
                case DebuffType.SlowAttack:
                    unit.StatModification<float>(Unit.UnitModStats.Attack, 1f);
                    break;
                case DebuffType.SlowMovement:
                    unit.StatModification<float>(Unit.UnitModStats.Move, 1f);
                    break;
            }
        }
    }
}

