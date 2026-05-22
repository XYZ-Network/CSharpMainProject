using System.Collections.Generic;
using System.Linq;
using Buffs;
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

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly BuffSystem _buffSystem;

        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;

            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);

            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _buffSystem = ServiceLocator.Get<BuffSystem>();
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

        private float GetModifiedMoveDelay()
        {
            float modifier = 1f;

            if (_buffSystem != null)
                modifier = _buffSystem.GetMoveSpeedModifier(this);

            modifier = Mathf.Max(0.05f, modifier);

            return Config.MoveDelay / modifier;
        }

        private float GetModifiedAttackDelay()
        {
            float modifier = 1f;

            if (_buffSystem != null)
                modifier = _buffSystem.GetAttackSpeedModifier(this);

            modifier = Mathf.Max(0.05f, modifier);

            return Config.AttackDelay / modifier;
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