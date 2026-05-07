using Model.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    public interface IModifiableUnit
    {
        UnitConfig Config { get; }
        float GetMoveDelay();
        float GetAttackDelay();
        float GetAttackRange();
        float GetAttackCount();
        void ApplyBuffModifier(BuffType type, float modifier);
        void RemoveBuffModifier(BuffType type);
    }
}
