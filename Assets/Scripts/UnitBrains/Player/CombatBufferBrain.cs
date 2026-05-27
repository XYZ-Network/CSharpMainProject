using System.Collections.Generic;
using System.Linq;
using Buffs;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class CombatBufferBrain : BaseUnitBrain
    {
        private const float BuffCooldown = 3f;
        private const float BuffDuration = 5f;
        private const float PauseBeforeBuff = 0.5f;
        private const float PauseAfterBuff = 0.5f;

        private const float MoveBuffModifier = 1.5f;
        private const float AttackBuffModifier = 1.5f;

        private float _buffCooldownLeft;
        private float _pauseBeforeLeft;
        private float _pauseAfterLeft;

        private IReadOnlyUnit _pendingTarget;

        private BuffSystem _buffSystem;
        private VFXView _vfxView;

        public override string TargetUnitName => "Combat Buffer";

        public override void Update(float deltaTime, float time)
        {
            if (_buffSystem == null)
                _buffSystem = ServiceLocator.Get<BuffSystem>();

            if (_vfxView == null)
                _vfxView = ServiceLocator.Get<VFXView>();

            if (_buffCooldownLeft > 0f)
                _buffCooldownLeft -= deltaTime;

            if (_pauseAfterLeft > 0f)
            {
                _pauseAfterLeft -= deltaTime;
                return;
            }

            if (_pauseBeforeLeft > 0f)
            {
                _pauseBeforeLeft -= deltaTime;

                if (_pauseBeforeLeft <= 0f)
                    ApplyPendingBuff();

                return;
            }

            if (_buffCooldownLeft > 0f)
                return;

            if (TryFindBuffTarget(out var target))
            {
                _pendingTarget = target;
                _pauseBeforeLeft = PauseBeforeBuff;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_pauseBeforeLeft > 0f || _pauseAfterLeft > 0f)
                return unit.Pos;

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        private bool TryFindBuffTarget(out IReadOnlyUnit target)
        {
            target = null;

            float rangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;

            var allies = runtimeModel.RoUnits
                .Where(u => u != unit)
                .Where(u => u.Config.IsPlayerUnit == unit.Config.IsPlayerUnit)
                .Where(u => !IsBufferUnit(u))
                .Where(u => (u.Pos - unit.Pos).sqrMagnitude <= rangeSqr)
                .Where(u => !_buffSystem.HasAnyBuff(u))
                .OrderBy(u => (u.Pos - unit.Pos).sqrMagnitude)
                .ToList();

            if (allies.Count == 0)
                return false;

            target = allies[0];
            return true;
        }

        private bool IsBufferUnit(IReadOnlyUnit otherUnit)
        {
            return otherUnit.Config.Name == TargetUnitName;
        }

        private void ApplyPendingBuff()
        {
            if (_pendingTarget != null &&
                _pendingTarget.Health > 0 &&
                _pendingTarget.Config.IsPlayerUnit == unit.Config.IsPlayerUnit &&
                !IsBufferUnit(_pendingTarget) &&
                (_pendingTarget.Pos - unit.Pos).sqrMagnitude <= unit.Config.AttackRange * unit.Config.AttackRange &&
                !_buffSystem.HasAnyBuff(_pendingTarget))
            {
                var buff = new UnitBuff(
                    BuffDuration,
                    MoveBuffModifier,
                    AttackBuffModifier);

                _buffSystem.AddBuff(_pendingTarget, buff);

                Debug.Log($"BUFF APPLIED from {unit.Config.Name} to {_pendingTarget.Config.Name}");

                if (_vfxView != null)
                    _vfxView.PlayVFX(_pendingTarget.Pos, VFXView.VFXType.BuffApplied);
            }

            _pendingTarget = null;
            _buffCooldownLeft = BuffCooldown;
            _pauseAfterLeft = PauseAfterBuff;
        }
    }
}