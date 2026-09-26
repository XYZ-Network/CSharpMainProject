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

        public float AttackRange =>
            Mathf.Max(0f, Config.AttackRange + _attackRangeModifier);

        public int ProjectileCount => _projectileCountModifier;

        private readonly List<BaseProjectile> _pendingProjectiles = new();

        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private float _moveSpeedModifier = 1f;
        private float _attackSpeedModifier = 1f;
        private float _attackRangeModifier = 0f;
        private int _projectileCountModifier = 1;

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;

            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);

            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime =
                    time + Config.BrainUpdateInterval;

                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                _nextMoveTime =
                    time + GetEffectiveMoveDelay();

                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime =
                    time + GetEffectiveAttackDelay();
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
                Debug.LogError(
                    $"Brain for unit {Config.Name} returned invalid move: {delta}");

                return;
            }

            if (_runtimeModel.RoMap[targetPos] ||
                _runtimeModel.RoUnits.Any(u => u.Pos == targetPos))
            {
                return;
            }

            Pos = targetPos;
        }

        public void ModifyMoveSpeed(float value)
        {
            if (value <= 0f)
                return;

            _moveSpeedModifier *= value;
        }

        public void ModifyAttackSpeed(float value)
        {
            if (value <= 0f)
                return;

            _attackSpeedModifier *= value;
        }

        public void ModifyAttackRange(float value)
        {
            _attackRangeModifier += value;
        }

        public void ModifyProjectileCount(int value)
        {
            if (value < 1)
                value = 1;

            _projectileCountModifier = value;
        }

        private float GetEffectiveMoveDelay()
        {
            return Config.MoveDelay /
                   Mathf.Max(0.0001f, _moveSpeedModifier);
        }

        private float GetEffectiveAttackDelay()
        {
            return Config.AttackDelay /
                   Mathf.Max(0.0001f, _attackSpeedModifier);
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