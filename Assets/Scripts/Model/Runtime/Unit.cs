using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Coordinators;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit, IBuffableUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public float AttackRange => Config.AttackRange * _attackRangeMultiplier;
        public UnitsCoordinator UnitsCoordinator { get; set; }
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;
        private float _moveSpeedMultiplier = 1f;
        private float _attackSpeedMultiplier = 1f;
        private float _attackRangeMultiplier = 1f;
        private int _extraAttackExecutions = 0;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        
        public Unit(UnitConfig config, Vector2Int startPos, UnitsCoordinator unitsCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            UnitsCoordinator = unitsCoordinator;
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
                _nextMoveTime = time + GetModifiedMoveDelay();
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + GetModifiedAttackDelay();
            }
        }

        private bool Attack()
        {
            var attackPerformed = false;
            var executions = Mathf.Max(1, 1 + _extraAttackExecutions);

            for (int i = 0; i < executions; i++)
            {
                var projectiles = _brain.GetProjectiles();
                if (projectiles == null || projectiles.Count == 0)
                {
                    if (!attackPerformed)
                    {
                        return false;
                    }

                    continue;
                }

                _pendingProjectiles.AddRange(projectiles);
                attackPerformed = true;
            }

            return attackPerformed;
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

        public void ApplyMoveSpeedMultiplier(float multiplier)
        {
            _moveSpeedMultiplier = _moveSpeedMultiplier * multiplier;
        }

        public void ApplyAttackSpeedMultiplier(float multiplier)
        {
            _attackSpeedMultiplier = _attackSpeedMultiplier * multiplier;
        }

        public void ModifyExtraAttackExecutions(int delta)
        {
            _extraAttackExecutions = Mathf.Max(0, _extraAttackExecutions + delta);
        }

        public void ApplyAttackRangeMultiplier(float multiplier)
        {
            _attackRangeMultiplier *= multiplier;
        }

        private float GetModifiedMoveDelay() => Config.MoveDelay / _moveSpeedMultiplier;

        private float GetModifiedAttackDelay() => Config.AttackDelay / _attackSpeedMultiplier;
    }
}
