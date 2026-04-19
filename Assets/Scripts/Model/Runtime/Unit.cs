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
        private Commander _commander;
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

        private float _baseMoveDelay;
        private float _baseAttackDelay;
        private float _baseAttackRange;

        private float _currentMoveDelay;
        private float _currentAttackDelay;
        private float _currentAttackRange;

        public float CurrentAttackRange => _currentAttackRange;

        public Unit(UnitConfig config, Vector2Int startPos, Commander commander)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _commander = commander;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
           
            _baseMoveDelay = config.MoveDelay;
            _baseAttackDelay = config.AttackDelay;
            _baseAttackRange = config.AttackRange;

            _currentMoveDelay = _baseMoveDelay;
            _currentAttackDelay = _baseAttackDelay;
            _currentAttackRange = _baseAttackRange;
            Debug.Log($"<color=cyan>[Unit] {Config.Name} создан. MoveDelay: {_currentMoveDelay}, AttackDelay: {_currentAttackDelay}</color>");
        }
        public BaseUnitBrain GetBrain()
        {
            return _brain;
        }
        public void ModifyMoveSpeed(float multiplier)
        {
            _currentMoveDelay = _baseMoveDelay / multiplier;
        }

        public void ModifyAttackSpeed(float multiplier)
        {
            _currentAttackDelay = _baseAttackDelay / multiplier;
        }

        public void ModifyAttackRange(float multiplier)
        {
            _currentAttackRange = _baseAttackRange * multiplier;
        }
        public Commander GetCommander()
        {
            return _commander;
        }
        public void Update(float deltaTime, float time)
        {
            
            if (IsDead)
                return;
            if (_currentMoveDelay <= 0) _currentMoveDelay = _baseMoveDelay;
            if (_currentAttackDelay <= 0) _currentAttackDelay = _baseAttackDelay;
            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                Debug.Log($"[Unit] {Config.Name} движение. Задержка: {_currentMoveDelay}, Время: {time}");
                _nextMoveTime = time + _currentMoveDelay;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + _currentAttackDelay;
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
            Debug.Log($"[Unit] {Config.Name} пытается двигаться с {Pos} на {targetPos}");
            if (delta.sqrMagnitude > 2)
            {
                return;
            }

            if (_runtimeModel.RoMap[targetPos] ||
                _runtimeModel.RoUnits.Any(u => u.Pos == targetPos))
            {
                return;
            }

            Pos = targetPos;
            Debug.Log($"<color=green>[Unit] {Config.Name} переместился на {Pos}</color>");
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