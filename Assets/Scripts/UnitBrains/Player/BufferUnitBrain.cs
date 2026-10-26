using System.Collections.Generic;
using System.Linq;
using Model.Buffs;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View;

namespace UnitBrains.Player
{
    public class BufferUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Buff Guardian"; // ← как в префабе

        private enum State { Moving, PreBuff, PostBuff }

        private const float BuffInterval = 3f;    // раз в 3 секунды
        private const float PrePostDelay = 0.5f;  // стоп до и после баффа
        private const float BuffDuration = 5f;    // длительность баффа
        private const float BuffMultiplier = 1.5f; // ускорение движения ×1.5

        private State _state = State.Moving;
        private float _stateTimer;
        private float _buffCooldown;
        private IReadOnlyUnit _buffTarget;

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
            base.Update(deltaTime, time); // тик координатора

            _buffCooldown -= deltaTime;

            switch (_state)
            {
                case State.Moving:
                    if (_buffCooldown <= 0f)
                    {
                        var ally = FindAllyToBuff();
                        if (ally != null)
                        {
                            _buffTarget = ally;
                            _state = State.PreBuff;
                            _stateTimer = PrePostDelay;
                        }
                        else
                        {
                            // Рядом нет цели для баффа — попробуем через мгновение
                            _buffCooldown = 0.2f;
                        }
                    }
                    break;

                case State.PreBuff:
                    _stateTimer -= deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        // На момент применения — ещё раз проверяем, что баффа нет
                        if (_buffTarget != null && !BuffSystem.Instance.HasBuff(_buffTarget))
                        {
                            BuffSystem.Instance.ApplyBuff(
                                _buffTarget,
                                BuffFactory.AttackSpeedUp(BuffDuration, BuffMultiplier));
                            PlayBuffVfx();
                        }
                        _state = State.PostBuff;
                        _stateTimer = PrePostDelay;
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

        /// <summary>
        /// Движение к точке сбора координатора через A*.
        /// В PreBuff / PostBuff стоим.
        /// </summary>
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

            // Не толкаемся в клетку, занятую другим юнитом
            if (runtimeModel.RoUnits.Any(u => u != unit && u.Pos == next))
                return unit.Pos;

            return next;
        }

        /// <summary>
        /// Стрельбу отключаем — пустой список целей.
        /// </summary>
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

        private void PlayBuffVfx()
        {
            if (_vfxView == null) TryGetVfxView();
            _vfxView?.PlayVFX(unit.Pos, VFXView.VFXType.BuffApplied);
        }
    }
}