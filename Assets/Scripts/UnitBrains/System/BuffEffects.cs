using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitBrains;
using UnitBrains.Player;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.UI.CanvasScaler;
using Unit = Model.Runtime.Unit;

namespace Assets.Scripts.UnitBrains.System
{
    public enum BuffType
    {
        FastAttack,
        FastMovement,
        IncreaseRange,
        DubleStrike

    }
    public class BuffEffects : IStatusEffect<BuffType>
    {
        private BuffType _name;
        private float _duration = 10f;
        private float _modificator = 1f;
        public BuffType Name => _name;
        Enum IStatusEffect.Name => Name;
        public float Duration => _duration;
        public float Modificator => _modificator;

        public BuffEffects(Unit unit)
        {    
            int rand = UnityEngine.Random.Range(0, 4);
            BuffType type = (BuffType)rand;
            SetBuff(type, unit);
        }
        public BuffEffects(BuffType type, Unit unit) 
        {
           SetBuff(type, unit);
        }
        private void SetBuff<T>(T type, Unit unit)
        {
            switch (type)
            {

                case BuffType.FastAttack:
                    _name = BuffType.FastAttack;
                    _modificator = 0.5f;
                    unit.StatModification<float>(Unit.UnitModStats.Attack, _modificator);
                    break;
                case BuffType.FastMovement:
                    _name = BuffType.FastMovement;
                    _modificator = 0.01f;
                    unit.StatModification<float>(Unit.UnitModStats.Move, _modificator);
                    break;
                case BuffType.IncreaseRange:
                    if(BuffFit(unit.Config.Name, BuffType.IncreaseRange))
                    {
                        _name = BuffType.IncreaseRange;
                        _modificator = 5f;
                        unit.StatModification<float>(Unit.UnitModStats.Range, _modificator);
                    }
                    break;
                case BuffType.DubleStrike:
                    if( BuffFit(unit.Config.Name, BuffType.DubleStrike))
                    {
                        _name = BuffType.DubleStrike;
                        unit.StatModification<bool>(Unit.UnitModStats.DubleStrike, true);
                    }
                    break;
            }
        }
        public bool BuffFit(string name, BuffType type)
        {
            if (type == BuffType.IncreaseRange && name != "Ironclad Behemoth")
            {
                return false;
            }
            if(type == BuffType.DubleStrike && name != "Cobra Commando")
            {
                return false;
            }
            return true;

        }
        public void BuffBreak(Unit unit)
        {
            switch (_name)
            {
                case BuffType.FastAttack:
                    unit.StatModification<float>(Unit.UnitModStats.Attack, 1f);
                    break;
                case BuffType.FastMovement:
                    unit.StatModification<float>(Unit.UnitModStats.Move, 1f);
                    break;
                case BuffType.IncreaseRange:
                    unit.StatModification<float>(Unit.UnitModStats.Range, 1f);
                    break;
                case BuffType.DubleStrike:
                    unit.StatModification<bool>(Unit.UnitModStats.DubleStrike, false);
                    break;
            }
        }
    }
}
