using Assets.Scripts.UnitBrains.Player;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
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
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        private float _moveSpeedMultiplier = 1f;
        private float _attackSpeedMultiplier = 1f;
        private int _projectileMultiplier = 1;
        private float _attackRangeMultiplier = 1f;

        internal void AddMoveSpeedMultiplier(float m) => _moveSpeedMultiplier *= m;
        internal void RemoveMoveSpeedMultiplier(float m) => _moveSpeedMultiplier /= m;

        internal void AddAttackSpeedMultiplier(float m) => _attackSpeedMultiplier *= m;
        internal void RemoveAttackSpeedMultiplier(float m) => _attackSpeedMultiplier /= m;

        internal void AddProjectileMultiplier(int k) => _projectileMultiplier *= k;
        internal void RemoveProjectileMultiplier(int k) => _projectileMultiplier /= k;

        internal void AddAttackRangeMultiplier(float m) => _attackRangeMultiplier *= m;
        internal void RemoveAttackRangeMultiplier(float m) => _attackRangeMultiplier /= m;

        public float CurrentAttackSpeedMultiplier => _attackSpeedMultiplier;
        public int CurrentProjectileMultiplier => _projectileMultiplier;
        public float CurrentAttackRange => Config.AttackRange * _attackRangeMultiplier;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;
        public BaseUnitBrain Brain => _brain;
        public override string ToString() => Config.Name;

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
                float moveMod = Mathf.Max(0.01f, _moveSpeedMultiplier);
                _nextMoveTime = time + Config.MoveDelay / moveMod;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                float atkMod = Mathf.Max(0.01f, _attackSpeedMultiplier);
                _nextAttackTime = time + Config.AttackDelay / atkMod;
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

        public void SetCoordinator(IUnitCoordinator coordinator)
        {
            _brain.SetCoordinator(coordinator);
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