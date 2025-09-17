using Model.Runtime;
using Model.Runtime.ReadOnly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Utilities;

namespace UnitBrains
{
    public interface IBuff<T> where T : IReadOnlyUnit
    {
        float Duration { get; }
        bool IsExpired { get; }
        bool CanApplyTo(T unit);
        void ApplyEffect(T unit);
        void RemoveEffect(T unit);
        void Update(float deltaTime);
        
    }

    public abstract class Buff<T> : IBuff<T> where T : IReadOnlyUnit
    {
        public float Duration { get; protected set; }
        protected float elapsedTime;

        protected Buff(float duration)
        {
            Duration = duration;
            elapsedTime = 0f;
        }

        public abstract void ApplyEffect(T unit);

        public abstract bool CanApplyTo(T unit);

        public abstract void RemoveEffect(T unit);

        public virtual void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
        }

        public bool IsExpired => elapsedTime >= Duration;
    }

    public class BuffsSys : MonoBehaviour
    {
        public Dictionary<IReadOnlyUnit, List<IBuff<IReadOnlyUnit>>> ActiveBuffs = new Dictionary<IReadOnlyUnit, List<IBuff<IReadOnlyUnit>>>();

        private void Awake()
        {
            ServiceLocator.RegisterAs(this, typeof(BuffsSys));
        }

        private void Update()
        {
            List<IReadOnlyUnit> unitsToRemove = new List<IReadOnlyUnit>();

            var unitKeys = new List<IReadOnlyUnit>(ActiveBuffs.Keys);

            foreach (var unit in unitKeys)
            {
                if (!ActiveBuffs.TryGetValue(unit, out var buffs) || buffs == null)
                    continue;

                for (int t = buffs.Count - 1; t >= 0; t--)
                {
                    var buff = buffs[t];
                    if (buff == null) 
                        continue;

                    buff.Update(Time.deltaTime);

                    if (buff.IsExpired)
                    {
                        if (buff is IBuff<IReadOnlyUnit> genericBuff) 
                            genericBuff.RemoveEffect(unit);
                        buffs.RemoveAt(t);
                    }
                }

                if (buffs.Count == 0)
                    unitsToRemove.Add(unit);
            }

            foreach (var unit  in unitsToRemove)
                ActiveBuffs.Remove(unit);
        }

        public void AddBuff<T>(T unit, IBuff<T> buff) where T : IReadOnlyUnit
        {
            if (!buff.CanApplyTo(unit))
                return;

            if (!ActiveBuffs.ContainsKey(unit))
                ActiveBuffs[unit] = new List<IBuff<IReadOnlyUnit>>();

            buff.ApplyEffect(unit);
            ActiveBuffs[unit].Add(buff as IBuff<IReadOnlyUnit>);
        }

        public bool HasBuffs(IReadOnlyUnit unit)
        {
            return ActiveBuffs.ContainsKey(unit) && ActiveBuffs[unit].Count > 0;
        }

        public IEnumerable<IBuff<IReadOnlyUnit>> GetBuffs(IReadOnlyUnit unit)
        {
            if (ActiveBuffs.TryGetValue(unit, out var buffs))
                return buffs;
            return new List<IBuff<IReadOnlyUnit>>();
        }
    }
}
