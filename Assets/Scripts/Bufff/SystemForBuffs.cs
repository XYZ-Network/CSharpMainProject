using Model;
using Model.Config;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitBrains;
using UnityEngine;
using Utilities;


public class SystemForBuffs
{
  
   Dictionary<Unit, BuDebu<Unit>> buf = new Dictionary<Unit, BuDebu<Unit>> ();
   private Coroutine coroutine;

    private void StartRountine(Unit unit, BuDebu<Unit> buDebu)
    {
        if (coroutine != null)
            return;
        this.coroutine = SingleCoroutines.StartRoutine(this.Pop(unit, buDebu));
    }
    public void StopRoutine()
    {
        SingleCoroutines.StopRoutine(this.coroutine);
        this.coroutine = null;
    }
    public float Push(Unit unit, BuDebu<Unit> buDebu)
    {
        if (buf.ContainsKey(unit))
        {
            return 0;
        }
        else
        {
            if(!buDebu.CanUseEffect(unit))
            {
                return 0;
            }
            
            buf.Add(unit, buDebu);
            buDebu.AddEffect(unit);
            StartRountine(unit, buDebu);
            //coroutine = StartCoroutine
            //coroutine(Pop(unit, buDebu));
            return buDebu.Value;

        }
        
    }
    public IEnumerator Pop(Unit unit, BuDebu<Unit> buDebu)
    {
       
        yield return new WaitForSeconds(buDebu.Duration);
        buDebu.RemoveEffect(unit);
        ////yield return new WaitForSeconds(buDebu.Duration);
        //buf.Remove(unit);
        ////yield return break;

    }
    public float IfTrueUnit(Unit unit, string name_of_buff)
    {
        if(buf.ContainsKey(unit) && (name_of_buff == buf[unit].NameOfBuff))
        {
            return buf[unit].Value;
        }
        else {return 0f; }
    }
    


}
