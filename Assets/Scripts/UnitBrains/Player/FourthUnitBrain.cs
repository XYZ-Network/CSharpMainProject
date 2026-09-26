using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Buffs;
using Model.Runtime.ReadOnly;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public sealed class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";

        private const float BuffInterval = 3f;
        private const float PauseBeforeBuff = 0.5f;
        private const float PauseAfterBuff = 0.5f;
        private const float BuffDuration = 6f;

        private const float MoveSpeedMultiplier = 1.5f;
        private const float AttackSpeedMultiplier = 5f;

        private float _nextBuffTime;
        private float _pauseUntil;
        private float _buffAtTime = -1f;

        private IReadOnlyUnit _pendingTarget;

        public override void Update(float deltaTime, float time)
        {
            if (_buffAtTime >= 0f)
            {
                if (time < _buffAtTime)
                    return;

                ApplyBuff(_pendingTarget);

                _pendingTarget = null;
                _buffAtTime = -1f;

                _pauseUntil =
                    time + PauseAfterBuff;

                _nextBuffTime =
                    time + BuffInterval;

                return;
            }

            if (time < _pauseUntil ||
                time < _nextBuffTime)
            {
                return;
            }

            var target = FindUnbuffedAlly();

            if (target == null)
                return;

            _pendingTarget = target;

            _pauseUntil =
                time + PauseBeforeBuff;

            _buffAtTime =
                time + PauseBeforeBuff;
        }

        public override Vector2Int GetNextStep()
        {
            if (Time.time < _pauseUntil ||
                _buffAtTime >= 0f)
            {
                return unit.Pos;
            }

            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            return new List<Vector2Int>();
        }

        private IReadOnlyUnit FindUnbuffedAlly()
        {
            var buffSystem =
                ServiceLocator.Get<IBuffSystem>();

            return GetUnitsInRadius(
                    unit.AttackRange,
                    true)

                .Where(
                    ally =>
                        ally.Config.IsPlayerUnit ==
                        unit.Config.IsPlayerUnit)

                .Where(
                    ally =>
                        ally is Model.Runtime.Unit concreteUnit &&
                        !buffSystem.HasBuff(concreteUnit))

                .OrderBy(
                    ally =>
                        (ally.Pos - unit.Pos)
                        .sqrMagnitude)

                .FirstOrDefault();
        }

        private void ApplyBuff(IReadOnlyUnit target)
        {
            if (target is not Model.Runtime.Unit concreteTarget)
                return;

            if (concreteTarget.IsDead)
                return;

            var buffSystem =
                ServiceLocator.Get<IBuffSystem>();

            if (buffSystem.HasBuff(concreteTarget))
                return;

            var buff =
                new SpeedBuff<Model.Runtime.Unit>(
                    BuffDuration,
                    MoveSpeedMultiplier,
                    AttackSpeedMultiplier);

            if (!buffSystem.AddBuff(concreteTarget, buff))
                return;

            ServiceLocator
                .Get<VFXView>()
                .PlayVFX(
                    concreteTarget.Pos,
                    VFXView.VFXType.BuffApplied);
        }
    }
}