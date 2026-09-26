using System.Collections.Generic;
using Model.Runtime.ReadOnly;
using Model.Runtime;

namespace Model.Runtime.Buffs
{
    public interface IBuffSystem
    {
        void AddBuff(Unit unit, UnitBuff buff);
        bool RemoveBuff(Unit unit, UnitBuff buff);
        void ClearBuffs(Unit unit);

        float GetMoveSpeedMultiplier(IReadOnlyUnit unit);
        float GetAttackSpeedMultiplier(IReadOnlyUnit unit);
        IReadOnlyList<UnitBuff> GetBuffs(IReadOnlyUnit unit);
        bool HasBuff(IReadOnlyUnit unit);
    }
}
