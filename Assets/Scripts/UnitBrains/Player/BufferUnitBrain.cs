using Assets.Scripts.Model.Runtime.Buffs;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using View;
using Unit = Model.Runtime.Unit;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public float BuffCooldown = 6f;      // интервал между баффами
        public float BuffDuration = 5f;      // длительность баффа
        public float BuffMultiplier = 1.25f;   // × увеличение баффа

        public float PauseBeforeCast = 0.5f;    // стоим до баффа
        public float PauseAfterCast = 0.5f;    // стоим после баффа
        public float InitialBuffDelay = 1f;      // смещение первого каста

        public int DesiredGap = 1;       // дистанция до союзника

        private enum CastState
        {
            None,
            WindUp,
            BackSwing
        }

        public override string TargetUnitName => "BufferUnit";
        public override bool IsPlayerUnitBrain => true;

        private float _nextBuffTime;
        private float _stateTimer;
        private CastState _castState;

        private bool _initDone;          // ленивый запуск таймера

        private Unit _buffTarget;
        private Unit _followTarget;

        private BuffSystem _buffSystem;
        private VFXView _vfxView;

        protected override void GenerateProjectiles(Vector2Int _, List<BaseProjectile> __) { }

        public override Vector2Int GetNextStep()
        {
            // во время пауз (до и после каста) юнит стоит
            if (_castState != CastState.None && _stateTimer > 0f)
                return unit.Pos;

            // следует за выбранным союзником
            if (_followTarget is { IsDead: false })
            {
                if ((unit.Pos - _followTarget.Pos).sqrMagnitude <= DesiredGap * DesiredGap)
                    return base.GetNextStep();

                Vector2Int dir = new(
                    Mathf.Clamp(_followTarget.Pos.x - unit.Pos.x, -1, 1),
                    Mathf.Clamp(_followTarget.Pos.y - unit.Pos.y, -1, 1));

                Vector2Int step = unit.Pos + dir;

                bool taken = runtimeModel.RoUnits.Any(u => u.Config.IsPlayerUnit && u.Pos == step);
                return taken ? unit.Pos : step;
            }

            return base.GetNextStep();
        }

        public override void Update(float dt, float time)
        {
            if (!_initDone)
            {
                _nextBuffTime = time + InitialBuffDelay;
                _initDone = true;
            }

            _buffSystem ??= ServiceLocator.Get<BuffSystem>();
            _vfxView ??= ServiceLocator.Get<VFXView>();

            if (_stateTimer > 0f) _stateTimer -= dt;

            switch (_castState)
            {
                case CastState.WindUp when _stateTimer <= 0f:
                    ApplyBuff(_buffTarget);
                    _buffTarget = null;
                    _castState = CastState.BackSwing;
                    _stateTimer = PauseAfterCast;
                    break;

                case CastState.BackSwing when _stateTimer <= 0f:
                    _castState = CastState.None;
                    break;
            }

            if (_castState == CastState.None && time >= _nextBuffTime)
            {
                Unit target = FindAllyWithoutBuff();
                if (target != null)
                {
                    _buffTarget = target;
                    _castState = CastState.WindUp;
                    _stateTimer = PauseBeforeCast;
                }
            }
            base.Update(dt, time);
        }

        private Unit FindAllyWithoutBuff()
        {
            float range2 = unit.Config.AttackRange * unit.Config.AttackRange;

            bool NeedBuff(Unit u) =>
                u.Brain is not BufferUnitBrain &&
                u.Config.IsPlayerUnit &&
                (u.Pos - unit.Pos).sqrMagnitude <= range2 &&
                !_buffSystem.HasBuff<HasteAttackBuff>(u);

            Unit target = runtimeModel.RoUnits
                .OfType<Unit>()
                .Where(NeedBuff)
                .OrderBy(u => (u.Pos - unit.Pos).sqrMagnitude)
                .FirstOrDefault();

            _followTarget = target ?? _followTarget;
            return target;
        }

        private void ApplyBuff(Unit t)
        {
            _buffSystem.AddBuff(t, new HasteAttackBuff(BuffDuration, BuffMultiplier));   // только 1 юнит
            _buffSystem.AddBuff(t, new DoubleShotBuff(BuffDuration));   // только 2 юнит
            _buffSystem.AddBuff(t, new IncreaseRangeBuff(BuffDuration));   // только 3 юнит
            _vfxView.PlayVFX(t.Pos, VFXView.VFXType.BuffApplied);
            _nextBuffTime = Time.time + BuffCooldown;
        }
    }
}