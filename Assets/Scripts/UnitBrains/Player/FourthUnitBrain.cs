using System.Collections.Generic;
using UnityEngine;
using Utilities;
using View;
using Model.Runtime;

namespace UnitBrains.Player
{
    public class FourthUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buffer";

        private enum BuffState { Idle, PrePause, PostPause }
        private BuffState _state = BuffState.Idle;
        private float _stateEndTime = 0f;

        private readonly float _buffInterval = 4f;
        private readonly float _buffDuration = 5f;
        private const float PauseLength = 0.5f;

        private float _nextBuffTime = 2f;

        private IBuffSystem _buffSystem;
        private VFXView _vfxView;

        private void EnsureServices()
        {
            if (_buffSystem == null)
                ServiceLocator.TryGet(out _buffSystem);
            if (_vfxView == null)
                ServiceLocator.TryGet(out _vfxView);
        }

        public override Vector2Int GetNextStep()
        {
            if (_state != BuffState.Idle)
                return unit.Pos;

            return base.GetNextStep();
        }

        public override void Update(float deltaTime, float time)
        {
            EnsureServices();

            if (_state == BuffState.Idle)
            {
                if (time >= _nextBuffTime)
                {
                    _state = BuffState.PrePause;
                    _stateEndTime = time + PauseLength;
                }
            }
            else if (_state == BuffState.PrePause)
            {
                if (time >= _stateEndTime)
                {
                    TryApplyBuff();

                    _vfxView?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);

                    _state = BuffState.PostPause;
                    _stateEndTime = time + PauseLength;

                    _nextBuffTime = time + _buffInterval;
                }
            }
            else if (_state == BuffState.PostPause)
            {
                if (time >= _stateEndTime)
                {
                    _state = BuffState.Idle;
                }
            }
        }

        private void TryApplyBuff()
        {
            var allies = GetUnitsInRadius(unit.Config.AttackRange, enemies: false);
            if (allies == null || allies.Count == 0)
                return;

            if (_buffSystem == null)
                return;

            foreach (var a in allies)
            {
                if (a == unit)
                    continue;

                if (a is Unit targetUnit)
                {
                    var moveMult = _buffSystem.GetMovementMultiplier(targetUnit);
                    var attackMult = _buffSystem.GetAttackMultiplier(targetUnit);

                    if (Mathf.Abs(moveMult - 1f) < 0.0001f && Mathf.Abs(attackMult - 1f) < 0.0001f)
                    {
                        _buffSystem.AddSpeedBuff(targetUnit, _buffDuration);
                        return;
                    }
                }
            }
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<Model.Runtime.Projectiles.BaseProjectile> intoList)
        {
        }
    }
}
