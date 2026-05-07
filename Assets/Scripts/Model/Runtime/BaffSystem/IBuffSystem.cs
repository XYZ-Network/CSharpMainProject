using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Runtime;

namespace BuffSystem
{
    public interface IBuffSystem
    {
        void AddBuff(IModifiableUnit unit, BuffType type, float duration, float modifier);
        bool HasAnyBuff(Unit unit);
    }
}