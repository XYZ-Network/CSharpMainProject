using System;
using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private float _moveDelayMultiplier = 1f;
        private float _attackDelayMultiplier = 1f;
        private float _attackRangeMultiplier = 1f;
        private int _extraShots = 0;

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        public void SetBrainCoordinator(UnitBrains.Player.IPlayerUnitsCoordinator coordinator)
        {
            _brain?.SetCoordinator(coordinator);
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
                _nextMoveTime = time + GetMoveDelay();
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + GetAttackDelay();
            }
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

        public void SetMoveDelayMultiplier(float mult)
        {
            _moveDelayMultiplier = mult;
        }

        public void SetAttackDelayMultiplier(float mult)
        {
            _attackDelayMultiplier = mult;
        }

        public void SetAttackRangeMultiplier(float mult)
        {
            _attackRangeMultiplier = mult;
        }

        public void SetExtraShots(int extra)
        {
            _extraShots = extra;
        }

        public float GetMoveDelay() => Config.MoveDelay * _moveDelayMultiplier;
        public float GetAttackDelay() => Config.AttackDelay * _attackDelayMultiplier;
        public float GetAttackRange() => Config.AttackRange * _attackRangeMultiplier;
        public int GetShotsCount() => Math.Max(1, 1 + _extraShots);
    }
}