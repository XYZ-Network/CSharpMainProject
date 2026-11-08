using System.Collections.Generic;
using System.Linq;
using Model.Buffs;
using Model.Runtime;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buff Guardian";

        private enum State { Moving, PreBuff, PostBuff }

        private const float BuffInterval = 3f;
        private const float PrePostDelay = 0.5f;
        private const float BuffDuration = 5f;
        private const float BuffMultiplier = 1.5f;

        private State _state = State.Moving;
        private float _stateTimer;
        private float _buffCooldown;
        private IReadOnlyUnit _buffTarget;
        private Buff _pendingBuff;           // ← ДОБАВЛЕНО

        private AStarUnitPath _path;
        private VFXView _vfxView;

        public BufferUnitBrain()
        {
            TryGetVfxView();
        }

        private void TryGetVfxView()
        {
            if (_vfxView != null) return;

            if (ServiceLocator.Contains<VFXView>())
                _vfxView = ServiceLocator.Get<VFXView>();
            else
                _vfxView = Object.FindObjectOfType<VFXView>();
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            _buffCooldown -= deltaTime;

            switch (_state)
            {
                case State.Moving:
                    if (_buffCooldown <= 0f)
                    {
                        var ally = FindAllyToBuff();
                        if (ally != null)
                        {
                            var buff = CreateBuffFor(ally);      // ← ИЗМЕНЕНО
                            if (buff != null)
                            {
                                _buffTarget = ally;
                                _pendingBuff = buff;
                                _state = State.PreBuff;
                                _stateTimer = PrePostDelay;
                            }
                            else _buffCooldown = 0.2f;
                        }
                        else _buffCooldown = 0.2f;
                    }
                    break;

                case State.PreBuff:
                    _stateTimer -= deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        if (_buffTarget != null && _pendingBuff != null)
                        {
                            BuffSystem.Instance.ApplyBuff(_buffTarget, _pendingBuff); // ← ИЗМЕНЕНО
                            PlayBuffVfx();
                        }
                        _state = State.PostBuff;
                        _stateTimer = PrePostDelay;
                        _pendingBuff = null;
                    }
                    break;

                case State.PostBuff:
                    _stateTimer -= deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        _state = State.Moving;
                        _buffCooldown = BuffInterval;
                        _buffTarget = null;
                    }
                    break;
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_state != State.Moving)
                return unit.Pos;

            var coord = PlayerUnitCoordinator.Instance;
            if (!coord.RecommendedPoint.HasValue)
                return unit.Pos;

            var dest = coord.RecommendedPoint.Value;
            if (unit.Pos == dest)
                return unit.Pos;

            if (_path == null || _path.EndPoint != dest)
                _path = new AStarUnitPath(runtimeModel, unit.Pos, dest);

            var next = _path.GetNextStepFrom(unit.Pos);

            if (runtimeModel.RoUnits.Any(u => u != unit && u.Pos == next))
                return unit.Pos;

            return next;
        }

        protected override List<Vector2Int> SelectTargets() => new();

        // ---------- Вспомогательное ----------

        private IReadOnlyUnit FindAllyToBuff()
        {
            var allies = GetUnitsInRadius(unit.Config.AttackRange, enemies: false);
            foreach (var ally in allies)
            {
                if (!BuffSystem.Instance.HasBuff(ally))
                    return ally;
            }
            return null;
        }

        /// <summary>
        /// Выбирает бафф по типу юнита союзника.
        /// Возвращает null, если бафф применить нельзя.
        /// </summary>
        private Buff CreateBuffFor(IReadOnlyUnit ally)
        {
            if (ally is not Unit u) return null;
            if (BuffSystem.Instance.HasBuff(ally)) return null;

            return u.Brain switch
            {
                SecondUnitBrain => BuffFactory.DoubleShot(BuffDuration),
                ThirdUnitBrain => BuffFactory.IncreasedRange(BuffDuration),
                _ => BuffFactory.MoveSpeedUp(BuffDuration, BuffMultiplier),
            };
        }

        private void PlayBuffVfx()
        {
            if (_vfxView == null) TryGetVfxView();
            _vfxView?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
        }
    }
}