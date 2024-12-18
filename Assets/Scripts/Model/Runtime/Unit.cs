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
        public bool IsBuff => _isBuff;
        public float MoveDelay => _moveDelay;
        public float AttackDelay => _attackDelay;

        public BaseUnitPath ActivePath => _brain?.ActivePath;

        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        public TargetRecommendationManager CurrentTargetRecommendationManager;

        public bool IsDoubleShot => _isDoubleShot;
        public float AttackRange => _attackRange;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        private bool _isBuff;

        private float _moveDelay;
        private float _attackDelay;
        private float _attackRange;
        public bool _isDoubleShot;

        public Unit(UnitConfig config, Vector2Int startPos, TargetRecommendationManager targetRecommendationManager)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            CurrentTargetRecommendationManager = targetRecommendationManager;

            SetMoveDelay(config.MoveDelay);
            SetAttackDelay(config.AttackDelay);
            SetDoubleShoot(false);
            SetAttackRange(config.AttackRange);
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
                _nextMoveTime = time + _moveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + _attackDelay;
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

        public void SetMoveDelay(float moveDelay) 
        {
            _moveDelay = moveDelay;
        }

        public void SetAttackDelay(float attackDelay)
        {
            _attackDelay = attackDelay;
        }

        public void SetBuff(bool isBuff)
        {
            _isBuff = isBuff;
        }

        public void SetDoubleShoot(bool isDoubleShot)
        {
            _isDoubleShot = isDoubleShot;
        }

        public void SetAttackRange(float attackRange)
        {
            _attackRange = attackRange;
        }
    }
}