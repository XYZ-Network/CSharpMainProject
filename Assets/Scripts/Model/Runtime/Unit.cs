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

        private float baseMoveDelay;
        private float baseAttackDelay;
        private float baseAttackRange;
        private bool canDoubleAttack = false;
        private float currentMoveDelay;
        private float currentAttackDelay;
        private float currentAttackRange;

        public Unit(UnitConfig config, Vector2Int startPos, UnitCoordinatorService unitCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);

            baseMoveDelay = config.MoveDelay;
            baseAttackDelay = config.AttackDelay;
            baseAttackRange = config.AttackRange;

            currentMoveDelay = baseMoveDelay;
            currentAttackDelay = baseAttackDelay;
            currentAttackRange = baseAttackRange;

            if (unitCoordinator != null) _brain.SetCoordinator(unitCoordinator);

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
                _nextMoveTime = time + GetModifierMoveDelay();
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + GetModifierAttackDelay();
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

        public void ModifyMoveDelay(float multiplier)
        {
            currentMoveDelay = baseMoveDelay / multiplier;
        }

        public void ModifyAttackDelay(float multiplier)
        {
            currentAttackDelay = baseAttackDelay / multiplier;
        }

        public void ModifyAttackRange(float multiplier)
        {
            currentAttackRange = baseAttackRange * multiplier;
        }

        public void EnableDoubleAttack(bool enable)
        {
            canDoubleAttack = enable;
        }

        private float GetModifierMoveDelay()
        {
            return currentMoveDelay;
        }

        private float GetModifierAttackDelay()
        {
            return currentAttackDelay;
        }

        public float GetCurrentAttackRange() => currentAttackRange;

        public bool CanDoubleAttack() => canDoubleAttack;
    }
}