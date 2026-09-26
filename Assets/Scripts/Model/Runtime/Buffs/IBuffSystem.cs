namespace Model.Runtime.Buffs
{
    public interface IBuffSystem
    {
        bool AddBuff(Unit unit, IBuff buff);
        bool HasBuff(Unit unit);
        void ClearBuffs(Unit unit);
    }
}