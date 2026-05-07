using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    public class BuffFactory
    {
        //private readonly Dictionary<BuffType, Func<float, float, Buff<IModifiableUnit>>> _creators =
        //    new Dictionary<BuffType, Func<float, float, Buff<IModifiableUnit>>();

        //public BuffFactory()
        //{
        //    Register(BuffType.MoveSpeed, (duration, modifier) => new MoveSpeedBuff(duration, modifier));
        //    Register(BuffType.AttackSpeed, (duration, modifier) => new AttackSpeedBuff(duration, modifier));
        //    Register(BuffType.AttackRange, (duration, modifier) => new AttackRangeBuff(duration, modifier));
        //}

        //public void Register(BuffType type, Func<float, float, Buff<IModifiableUnit>> creator)
        //{
        //    _creators[type] = creator;
        //}

        //public Buff<IModifiableUnit> Create(BuffType type, float duration, float modifier)
        //{
        //    if (_creators.TryGetValue(type, out var creator))
        //        return creator(duration, modifier);
        //    throw new ArgumentException($"No buff registered for type: {type}");
        //}
    }
}