using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitBrains;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Baffs
{
    abstract public class BaseStatusEffect
    {
        virtual public int Order => 0;

        virtual public float Time => 5f;

        abstract public void Effect(BaseUnitBrain brain);

        abstract public void Diseffect(BaseUnitBrain brain);

        virtual public bool CanAddStatusToData(List<List<BaseStatusEffect>> buffs)
        {
            if (buffs.SelectMany(u => u).Select<BaseStatusEffect, Action<BaseUnitBrain>>(u => u.Effect).Any(u => AreDelegatesIdentical(u, this.Effect)))
            {
                Debug.Log("Не может добавить бафф на юнита");
                return false;
            }
            return true;
        }
        bool AreDelegatesIdentical(Action<BaseUnitBrain> d1, Action<BaseUnitBrain> d2)
        {
            Debug.Log(d1.Target);
            Debug.Log(d2.Target);
            Debug.Log(d1.Method);
            Debug.Log(d2.Method);

            return d1.Method == d2.Method;
        }

        public virtual IEnumerator StartStatus(BaseUnitBrain brain)
        {
            yield return new WaitForSecondsRealtime(Time);
            Debug.Log("Buff exspired so i delete it");
            BuffsData.DeleteStatusFromData(brain, this);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            BaseStatusEffect other = (BaseStatusEffect)obj;
            return AreDelegatesIdentical(Effect, other.Effect) &&
                   AreDelegatesIdentical(Diseffect, other.Diseffect) &&
                   Order == other.Order;
        }
    }

}
