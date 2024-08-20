using Model.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSystem 
{
    private Dictionary<Unit,List<IBuff<Unit>>> _buffs = new Dictionary<Unit,List<IBuff<Unit>>>();
    public Dictionary<Unit, List<IBuff<Unit>>> Buffs => _buffs;
    public void AddBuff(Unit unit, IBuff<Unit> buff)
    {
        if(buff.CanBeAdd(unit))
        {
            if (!_buffs.ContainsKey(unit))
            {
                _buffs[unit] = new List<IBuff<Unit>>();
            }
            _buffs[unit].Add(buff);
        }
        
    }

    public void Update()
    {
        foreach(var buff in _buffs)
        {
            for (int i = 0; i < buff.Value.Count; i++) 
            {
                buff.Value[i].UpdateDuration(buff.Key,Time.deltaTime);
            }
        }
    }


    
}
