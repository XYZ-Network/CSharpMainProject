using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Runtime;
using Assets.Scripts.UnitBrains;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit, IUnitModifiable
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

        private float _moveSpeedModifier = 1f;
        private float _attackSpeedModifier = 1f;
        private float _attackRangeModifier = 1f;
        private bool _doubleShotEnabled = false;

        public float CurrentMoveSpeedModifier => _moveSpeedModifier;
        public float CurrentAttackSpeedModifier => _attackSpeedModifier;
        public float CurrentAttackRangeModifier => _attackRangeModifier;
        public bool IsDoubleShotEnabled => _doubleShotEnabled;

        public float AttackRange => Config.AttackRange / CurrentAttackRangeModifier;

        public float MoveDelay => Config.MoveDelay / CurrentMoveSpeedModifier;

        public float AttackDelay => Config.AttackDelay / CurrentAttackSpeedModifier;

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
        }

        public void ModifyMoveSpeed(float modifier) => _moveSpeedModifier *= modifier;
        public void ModifyAttackSpeed(float modifier) => _attackSpeedModifier *= modifier;
        public void ModifyAttackRange(float modifier) => _attackRangeModifier *= modifier;
        public void EnableDoubleShot() => _doubleShotEnabled = true;
        public void DisableDoubleShot() => _doubleShotEnabled = false;
        
        public void SetCoordinator(UnitCoordinator coordinator)
        {
            _brain.Coordinator = coordinator;
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
                var moveDelay = Config.MoveDelay / _moveSpeedModifier;
                _nextMoveTime = time + moveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                var attackDelay = Config.AttackDelay / _attackSpeedModifier;
                _nextAttackTime = time + attackDelay;
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
    }
}