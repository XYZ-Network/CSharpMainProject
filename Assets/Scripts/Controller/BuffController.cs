using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Buff;
using Utilities;

namespace Controller
{
    public class BuffController<T> : IDisposable where T : IBuffable
    {
        Dictionary<Unit, List<T>> _unitBuffs = new();
        private TimeUtil _time = ServiceLocator.Get<TimeUtil>();

        public BuffController()
        {
            _time.AddFixedUpdateAction(UpdateCoroutine);
        }

        public void AddUnitBuff(Unit unit, T buff)
        {
            if (!_unitBuffs.ContainsKey(unit))
            {
                _unitBuffs.Add(unit, new List<T>());
            }
            // TODO check if buff can be applied to unit
            
            if (buff.CanApplyToUnit() && !_unitBuffs[unit].Contains(buff))
            {
                buff.Activate();
                _unitBuffs[unit].Add(buff);
            }
        }

        public List<T> GetUnitBuffs(Unit unit)
        {
            return (_unitBuffs.ContainsKey(unit)) ? _unitBuffs[unit] : new();
        }
        private void UpdateCoroutine(float timeDelta)
        {
            if (_unitBuffs.Values.Count == 0)
                return;
            foreach (var unitBuffList in _unitBuffs.Values)
            {
                for (int i = 0; i < unitBuffList.Count; i++)
                {
                    T unitBuff = unitBuffList[i];
                    unitBuff.ReduceDurationByDelta(timeDelta);
                    if (!unitBuff.IsActive())
                        unitBuff.Dispose();
                        unitBuffList.Remove(unitBuff);
                }
            }
        }

        public void Dispose()
        {
            if (_unitBuffs.Values.Count > 0)
            {
                foreach (var buffList in _unitBuffs.Values)
                {
                    foreach (var buff in buffList)
                    {
                        buff.Dispose();
                    }
                }
                _unitBuffs.Clear();
            }
            _time.RemoveFixedUpdateAction(UpdateCoroutine);
        }
    }
}
