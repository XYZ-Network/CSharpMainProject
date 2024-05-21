using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnityEngine;

public class BuffSystem 
{
    private Dictionary<Unit, List<Buff>> _buffs = new Dictionary<Unit, List<Buff>>();

    public void AddBuff(Unit unit, Buff buff)
    {
        if (!_buffs.ContainsKey(unit))
        {
            _buffs[(Unit)unit] = new List<Buff>();
        }
        _buffs[unit].Add(buff);
    }

    public bool HasBuff(Unit unit)
    {
        if (_buffs.TryGetValue(unit, out List<Buff> buffs))
        {
            return buffs.Count > 0;
        }
        return false;
    }

    public void Update()
    {
        foreach (var pair in _buffs)
        {
            for (int i = pair.Value.Count - 1; i >= 0; i--)
            {
                pair.Value[i].ReduceDuration(Time.deltaTime);
                if (pair.Value[i].Duration <= 0)
                {
                    pair.Value.RemoveAt(i);
                }
            }
        }
    }

    public float GetMoveSpeedModifier(Unit unit)
    {
        float modifier = 1.0f;
        if (_buffs.ContainsKey(unit))
        {
            foreach (var buff in _buffs[unit])
            {
                modifier *= buff.MoveSpeedModifier;
            }
        }
        return modifier;
    }

    public float GetAttackSpeedModifier(Unit unit)
    {
        float modifier = 1.0f;
        if (_buffs.ContainsKey(unit))
        {
            foreach (var buff in _buffs[unit])
            {
                modifier *= buff.AttackSpeedModifier;
            }
        }
        return modifier;
    }



}
