using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public float MoveDelay { get; set; }
        public float AttackDelay { get; set; }
        public float AttackRange { get; set; }
        public int AttackCount { get; set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;
        public Type GetBrainType => _brain.GetType();

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        
        public Unit(UnitConfig config, Vector2Int startPos, ActionGenerator actionGenerator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetActionGenerator(actionGenerator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            ResetAttackDelayToDefault();
            ResetMoveDelayToDefault();
            ResetAttackRangeToDefault();
            ResetAttackCountToDefault();
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
                _nextMoveTime = time + MoveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + AttackDelay;
            }
        }


        private bool Attack()
        {
            var projectiles = _brain.GetProjectiles();
            if (projectiles == null || projectiles.Count == 0)
                return false;
            
            for (int i = 0; i < AttackCount; i++)
            {
                _pendingProjectiles.AddRange(projectiles);
            }
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

        public void ResetAttackDelayToDefault()
        {
            MoveDelay = Config.MoveDelay;
        }

        public void ResetMoveDelayToDefault()
        {
            AttackDelay = Config.AttackDelay;
        }

        public void ResetAttackRangeToDefault()
        {
            AttackRange = Config.AttackRange;
        }

        public void ResetAttackCountToDefault()
        {
            AttackCount = 1;
        }
    }
}