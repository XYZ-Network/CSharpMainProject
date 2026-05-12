using System.Collections.Generic;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
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

        private static readonly HashSet<IReadOnlyUnit> _buffedUnits = new();

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

                    var target = FindUnbuffedAlly();
                    if (target == null)
                        return;

                    _pendingTarget = target;
                    _state = BufferState.Preparing;
                    _stateEndTime = time + StopBeforeBuff;
                    break;

                case BufferState.Preparing:
                    if (time < _stateEndTime)
                        return;

                    if (_pendingTarget != null && _pendingTarget.Health > 0 && IsTargetInRange(_pendingTarget.Pos))
                        ApplyBuff(_pendingTarget);

                    _pendingTarget = null;
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

        private IReadOnlyUnit FindUnbuffedAlly()
        {
            var allies = GetUnitsInRadius(unit.Config.AttackRange, enemies: false);
            foreach (var ally in allies)
            {
                if (!_buffedUnits.Contains(ally))
                    return ally;
            }

            return null;
        }

        private void ApplyBuff(IReadOnlyUnit target)
        {
            _buffedUnits.Add(target);
            ServiceLocator.Get<VFXView>().PlayVFX(target.Pos, VFXView.VFXType.BuffApplied);
        }
    }
}
