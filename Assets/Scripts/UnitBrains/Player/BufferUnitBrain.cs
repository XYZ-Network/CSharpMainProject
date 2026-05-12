using System.Collections.Generic;
using Model.Runtime;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Buffs;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Combat Medic";

        private const float BuffInterval = 3f;
        private const float StopBeforeBuff = 0.5f;
        private const float StopAfterBuff = 0.5f;

        private static readonly BaseBuff[] _availableBuffs =
        {
            new DoubleShotBuff(),
            new ExtendedRangeBuff(),
        };

        private enum BufferState
        {
            Idle,
            Preparing,
            Cooldown,
        }

        private BufferState _state = BufferState.Idle;
        private float _stateEndTime;
        private float _nextBuffTime;
        private IReadOnlyUnit _pendingTarget;
        private BaseBuff _pendingBuff;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        public override void Update(float deltaTime, float time)
        {
            switch (_state)
            {
                case BufferState.Idle:
                    if (time < _nextBuffTime)
                        return;

                    if (!TryFindBuffTarget(out var target, out var buff))
                        return;

                    _pendingTarget = target;
                    _pendingBuff = buff;
                    _state = BufferState.Preparing;
                    _stateEndTime = time + StopBeforeBuff;
                    break;

                case BufferState.Preparing:
                    if (time < _stateEndTime)
                        return;

                    TryApplyPendingBuff();

                    _pendingTarget = null;
                    _pendingBuff = null;
                    _state = BufferState.Cooldown;
                    _stateEndTime = time + StopAfterBuff;
                    break;

                case BufferState.Cooldown:
                    if (time < _stateEndTime)
                        return;

                    _state = BufferState.Idle;
                    _nextBuffTime = time + BuffInterval;
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_state != BufferState.Idle)
                return unit.Pos;

            return base.GetNextStep();
        }

        private bool TryFindBuffTarget(out IReadOnlyUnit target, out BaseBuff buff)
        {
            var allies = GetUnitsInRadius(unit.AttackRange, enemies: false);
            foreach (var ally in allies)
            {
                foreach (var candidate in _availableBuffs)
                {
                    if (!candidate.CanApplyTo(ally))
                        continue;

                    target = ally;
                    buff = candidate;
                    return true;
                }
            }

            target = null;
            buff = null;
            return false;
        }

        private void TryApplyPendingBuff()
        {
            if (_pendingTarget == null || _pendingBuff == null)
                return;

            if (!IsTargetInRange(_pendingTarget.Pos))
                return;

            if (_pendingTarget is not Unit mutableUnit)
                return;

            if (!_pendingBuff.CanApplyTo(mutableUnit))
                return;

            _pendingBuff.ApplyTo(mutableUnit);
            ServiceLocator.Get<VFXView>().PlayVFX(mutableUnit.Pos, VFXView.VFXType.BuffApplied);
        }
    }
}
