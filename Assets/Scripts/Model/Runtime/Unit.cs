using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.BuffsAndDebuffs;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using View;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit
    {
        public UnitConfig Config { get; private set; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        public float RangeModifier => _rangeModifier;
        public bool DoubleShootMode => _doubleShootMode;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;
        private float _nextBrainUpdateTime = 0f;
        private float? _nextMoveTime = 0f;
        private float? _nextAttackTime = 0f;
        private float? _nextMoveTimeNew = null;
        private float? _nextAttackTimeNew = null;
        private float _rangeModifier = 1.0f;
        private bool _doubleShootMode = false;
        private BuffAndDebuffControllSystem _buffAndDebuffControllSystem;
        private VFXView _vfxView;

        public Unit(UnitConfig config, Vector2Int startPos, PathAndTargetCoordinator pathAndTargetCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetController(pathAndTargetCoordinator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _buffAndDebuffControllSystem = ServiceLocator.Get<BuffAndDebuffControllSystem>();
            _vfxView = ServiceLocator.Get<VFXView>();
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                _nextMoveTime = _nextMoveTimeNew == null ? (time + Config.MoveDelay) : _nextMoveTimeNew;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = _nextAttackTimeNew == null ? (time + Config.AttackDelay) : _nextAttackTimeNew;
            }

            if (_buffAndDebuffControllSystem.CheckUnitInEffectList(this))
            {
                _vfxView.PlayVFX(Pos, VFXView.VFXType.BuffApplied);
            }
        }

        public void SetNextAttackTime(float? modifier)
        {
            _nextAttackTimeNew = modifier;
        }

        public void SetNextMoveTime(float? modifier)
        {
            _nextMoveTimeNew = modifier;
        }

        public void SetRangeModifier(float modifier)
        {
            _rangeModifier = modifier;
        }

        public void SetDoubleShootMode(bool modifier)
        {
            _doubleShootMode = modifier;
        }

        private bool Attack()
        {
            var projectiles = _brain.GetProjectiles();
            if (projectiles == null || projectiles.Count == 0)
                return false;

            _pendingProjectiles.AddRange(projectiles);
            return true;
        }

        private void Move()
        {
            var targetPos = _brain.GetNextStep();
            var delta = targetPos - Pos;
            if (delta.sqrMagnitude > 2)
            {
                Debug.LogError($"Brain for unit {Config.Name} returned invalid move: {delta}");
                return;
            }

            if (_runtimeModel.RoMap[targetPos] ||
                _runtimeModel.RoUnits.Any(u => u.Pos == targetPos))
            {
                return;
            }

            Pos = targetPos;
        }

        public void ClearPendingProjectiles()
        {
            _pendingProjectiles.Clear();
        }

        public void TakeDamage(int projectileDamage)
        {
            Health -= projectileDamage;
        }
    }
}