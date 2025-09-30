using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Runtime;
using UnitBrains;
using UnitBrains.Player;
using UnityEngine;
using Utilities;

namespace Controller
{
    public class BuffsController : IDisposable
    {
        private class BuffInstance
        {
            public IBuff Buff { get; }
            public Unit Unit { get; }
            public Type BuffType { get; }

            public BuffInstance(IBuff buff, Unit unit)
            {
                Buff = buff;
                Unit = unit;
                BuffType = buff.GetType();
            }
        }

        private readonly List<BuffInstance> _activeBuffs = new();
        private readonly Action<float> _updateAction;
        private TimeUtil _timeUtil;
        private bool _disposed = false;

        public BuffsController()
        {
            if (!ServiceLocator.Contains<TimeUtil>())
                throw new InvalidOperationException("TimeUtil must be registered before BuffsController");

            _updateAction = Update;
            _timeUtil = ServiceLocator.Get<TimeUtil>();
            _timeUtil.AddFixedUpdateAction(_updateAction);
        }

        public void AddBuff(Unit unit, IBuff buff)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BuffsController));

            if (unit.IsDead || !buff.CanApply(unit))
                return;

            var existingBuffInstance = _activeBuffs.FirstOrDefault(b => b.Buff.GetType() == buff.GetType() && b.Unit == unit);
            
            if (existingBuffInstance != null)
            {
                existingBuffInstance.Buff.Duration = Mathf.Max(existingBuffInstance.Buff.Duration, buff.Duration);
            }
            else
            {
                buff.Apply(unit);
                _activeBuffs.Add(new BuffInstance(buff, unit));
            }
        }

        public void RemoveBuffs(Unit unit)
        {
            if (_disposed) return;

            var buffsToRemove = _activeBuffs.Where(b => b.Unit == unit).ToList();

            foreach (var buffInstance in buffsToRemove)
            {
                buffInstance.Buff.Remove(buffInstance.Unit);
                _activeBuffs.Remove(buffInstance);
            }
        }

        public void ClearAllBuffs()
        {
            if (_disposed) return;

            foreach (var buffInstance in _activeBuffs)
            {
                buffInstance.Buff.Remove(buffInstance.Unit);
            }
            _activeBuffs.Clear();
        }

        public void Update(float deltaTime)
        {
            if (_disposed) return;

            var buffsToRemove = new List<BuffInstance>();

            foreach (var buffInstance in _activeBuffs)
            {
                if (buffInstance.Unit.IsDead)
                {
                    buffsToRemove.Add(buffInstance);
                    continue;
                }

                buffInstance.Buff.Duration -= deltaTime;
                if (buffInstance.Buff.Duration <= 0)
                {
                    buffsToRemove.Add(buffInstance);
                }
            }

            foreach (var buffInstance in buffsToRemove)
            {
                buffInstance.Buff.Remove(buffInstance.Unit);
                _activeBuffs.Remove(buffInstance);
            }
        }

        public bool HasBuff<T>(Unit unit) where T : IBuff
        {
            return ! _disposed && _activeBuffs.Any(b => b.Unit == unit && b.Buff is T);
        }

        public void CheckAndApplyAttackBaseBuffs(Unit unit, Vector2Int target)
        {
            if (unit.IsDead) return;

            var brain = unit.GetBrain();
            if (brain == null) return;

            var baseAttackDetector = ServiceLocator.Get<IBaseAttackDetector>();
            if (!baseAttackDetector.IsAttackingEnemyBase(unit, target))
                return;

            if (brain is SecondUnitBrain)
            {
                AddBuff(unit, new DoubleShotBuff<Unit>(5f));
            }
            else if (brain is ThirdUnitBrain)
            {
                AddBuff(unit, new IncreasedRangeBuff<Unit>(5f, 1.5f));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _timeUtil?.RemoveFixedUpdateAction(_updateAction);
            ClearAllBuffs();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}