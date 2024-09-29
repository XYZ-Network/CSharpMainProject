using System.Collections.Generic;
using System.Linq;
using Model.Runtime;
using UnityEngine;

public enum BuffNames
{
    UpMoveSpeed,
    DownMoveSpeed,
    UpAttackSpeed,
    DownAttackSpeed
};

public class BuffSystem
{
    private Dictionary<Unit, List<AbstractBuff>> _unitBuffs = new Dictionary<Unit, List<AbstractBuff>>();

    public AbstractBuff[] getBuffs(Unit unit, BuffNames[] buffNames)
    {
        List<AbstractBuff> buffs = _unitBuffs.TryGetValue(unit, out List<AbstractBuff> buff) ? buff : new List<AbstractBuff>();
        _unitBuffs.TryAdd(unit, buffs);

        AbstractBuff[] foundedBuff = buffs.Where(buff => buffNames.Any((buffName) => buffName == buff.Type)).ToArray();
        return foundedBuff;
    }

     public AbstractBuff[] getBuffs(Unit unit)
    {
        List<AbstractBuff> buffs = _unitBuffs.TryGetValue(unit, out List<AbstractBuff> buff) ? buff : new List<AbstractBuff>();
        _unitBuffs.TryAdd(unit, buffs);

        return buffs.ToArray();
    }

    public void setBuff(Unit unit, AbstractBuff buff)
    {
        List<AbstractBuff> buffs = _unitBuffs.TryGetValue(unit, out List<AbstractBuff> foundedBuffs) ? foundedBuffs : new List<AbstractBuff>();
        buffs.Add(buff);
        _unitBuffs.TryAdd(unit, buffs);
    }

    public void UpdateBuffDuration()
    {
        foreach (var unit in _unitBuffs.Keys)
        {
            List<AbstractBuff> unitbuffs = _unitBuffs[unit];
            for (int i = 0; i < unitbuffs.Count; i++)
            {
                AbstractBuff buff = unitbuffs[i];
                buff.Duration -= Time.deltaTime;
                if (buff.Duration <= 0)
                {
                    _unitBuffs[unit].Remove(buff);
                }
            }
        }
    }
}