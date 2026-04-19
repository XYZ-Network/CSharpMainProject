using Model.Runtime.ReadOnly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class EffectSystem : MonoBehaviour
    {

        private readonly Dictionary<Unit, List<IBuff>> _activeBuffs = new();
        private readonly TimeUtil _timeUtil;

        public EffectSystem()
        {
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddUpdateAction(OnUpdate);
        }

        ~EffectSystem()
        {
            _timeUtil.RemoveUpdateAction(OnUpdate);
        }

        public void AddEffect(Unit target, IBuff buff)
        {
            buff.Apply(target);

            if (!_activeBuffs.ContainsKey(target))
            {
                _activeBuffs[target] = new List<IBuff>();
            }
            _activeBuffs[target].Add(buff);
        }

        public void RemoveEffect(Unit target, IBuff buff)
        {
            if (_activeBuffs.ContainsKey(target))
            {
                List<IBuff> buffs = _activeBuffs[target];
                buff.Remove(target);
                buffs.Remove(buff);

                if (buffs.Count == 0)
                {
                    _activeBuffs.Remove(target);
                }
            }
        }


        private void OnUpdate(float deltaTime)
        {
            List<Unit> unitsToRemove = new List<Unit>();
            foreach (Unit unit in _activeBuffs.Keys)
            {
                List<IBuff> buffs = _activeBuffs[unit];

                for (int i = 0; i < buffs.Count; i++)
                {
                    IBuff buff = buffs[i];
                    buff.TimeLeft -= deltaTime;

                    if (buff.TimeLeft <= 0)
                    {
                        buff.Remove(unit);
                        buffs.RemoveAt(i);
                        i--;
                    }
                }

                if (buffs.Count == 0)
                {
                    unitsToRemove.Add(unit);
                }
            }
            foreach (Unit unit in unitsToRemove)
            {
                _activeBuffs.Remove(unit);
            }
        }

        public bool HasActiveBuff(Unit target)
        {
            return _activeBuffs.ContainsKey(target) && _activeBuffs[target].Count > 0;
        }
    }

    }
   