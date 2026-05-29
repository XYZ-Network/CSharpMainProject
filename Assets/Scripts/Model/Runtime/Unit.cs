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
        public BaseUnitPath ActivePath => _brain.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        public float CurrentAttackRange => Config.AttackRange + _attackRangeBonus;
        public float CurrentMoveDelay => Config.MoveDelay / Mathf.Max(0.05f, _moveSpeedMultiplier);
        public float CurrentAttackDelay => Config.AttackDelay / Mathf.Max(0.05f, _attackSpeedMultiplier);
        public int AdditionalShots => _additionalShots;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private readonly IReadOnlyRuntimeModel _runtimeModel;

        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private float _moveSpeedMultiplier = 1f;
        private float _attackSpeedMultiplier = 1f;
        private float _attackRangeBonus = 0f;
        private int _additionalShots = 0;

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
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                _nextMoveTime = time + CurrentMoveDelay;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + CurrentAttackDelay;
            }
        }

        public void ResetBuffState()
        {
            _moveSpeedMultiplier = 1f;
            _attackSpeedMultiplier = 1f;
            _attackRangeBonus = 0f;
            _additionalShots = 0;
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            _moveSpeedMultiplier *= multiplier;
        }

        public void MultiplyAttackSpeed(float multiplier)
        {
            _attackSpeedMultiplier *= multiplier;
        }

        public void AddAttackRange(float bonus)
        {
            _attackRangeBonus += bonus;
        }

        public void AddAdditionalShots(int count)
        {
            _additionalShots += count;
        }

        private bool Attack()
        {
            bool fired = false;
            int volleys = 1 + AdditionalShots;

            Debug.Log($"{Config.Name} attack volleys = {volleys}");

            for (int i = 0; i < volleys; i++)
            {
                var projectiles = _brain.GetProjectiles();

                if (projectiles == null || projectiles.Count == 0)
                    continue;

                _pendingProjectiles.AddRange(projectiles);
                fired = true;
            }

            return fired;
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

            if (targetPos == Pos)
                return;

            if (_runtimeModel.RoMap[targetPos] ||
                _runtimeModel.RoUnits.Any(u => u != this && u.Pos == targetPos))
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