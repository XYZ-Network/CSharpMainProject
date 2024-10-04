using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;

public enum BuffNames
{
    UpMoveSpeed,
    DownMoveSpeed,
    UpAttackSpeed,
    DownAttackSpeed,
    DoubleAttackBuff,
    UpRangeBuff
};

public class BuffSystem
{
    private Dictionary<IReadOnlyUnit, List<AbstractBuff>> _unitBuffs = new Dictionary<IReadOnlyUnit, List<AbstractBuff>>();
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;

    public BuffSystem()
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _timeUtil.AddFixedUpdateAction(Update);
    }

    private void Update(float deltaTime)
    {
        UpdateBuffDuration();
        if (UnityEngine.Random.Range(0, 1) == 0)
        {
            SetRandomBuffForRandomUnit();
        }
    }

    private void SetRandomBuffForRandomUnit()
    {
        IReadOnlyUnit[] units = _runtimeModel.RoPlayerUnits.ToArray();
        if (units.Length == 0) return;

        int randomNumber = UnityEngine.Random.Range(0, units.Count());
        Unit randomUnit = (Unit)units[randomNumber];
        if (getBuffs(randomUnit).Length > 2) return;
        int randomBuff = UnityEngine.Random.Range(1, 10);
        switch (randomBuff)
        {
            case 1:
                setBuff(new UpRangeBuff(randomUnit));
                break;
            case 2:
                setBuff(new DownMoveSpeedBuff(randomUnit));
                break;
            case 3:
                setBuff(new UpAttackSpeedBuff(randomUnit));
                break;
            case 4:
                setBuff(new DownAttackSpeedBuff(randomUnit));
                break;
            case 5:
                setBuff(new DoubleAttackBuff(randomUnit));
                break;
            case 6:
                setBuff(new UpMoveSpeedBuff(randomUnit));
                break;
            default:
                // no buff for unit
                break;
        }
    }

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

    public void setBuff(AbstractBuff buff)
    {
        IReadOnlyUnit unit = buff.unit;
        List<AbstractBuff> buffs = _unitBuffs.TryGetValue(unit, out List<AbstractBuff> foundedBuffs) ? foundedBuffs : new List<AbstractBuff>();
        buffs.Add(buff);
        _unitBuffs.TryAdd(unit, buffs);
        buff.Apply();
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
                    buff.Dispose();
                    _unitBuffs[unit].Remove(buff);
                }
            }
        }
    }
}