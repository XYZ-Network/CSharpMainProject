using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts.UnitBrains;
using Assets.Scripts.UnitBrains.Buffs;
using Controller;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private float _attackSpeed = 1f;
        private float _moveSpeed = 1f;
        private bool _doubleShot = false;
        private float _attackRangeModifier = 1f;
        public BaseUnitBrain Brain => _brain;
        public bool DoubleShot => _doubleShot;
        public float AttackRangeMultiplier => _attackRangeModifier;
        public int ID { get; private set; }

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            if (config.Cost == 70)
                ID = SetID();
        }
        private static int _idValue = 0;
        private static int SetID()
        {
            _idValue++;
            return _idValue - 1;
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
                _nextMoveTime = time + Config.MoveDelay * _moveSpeed;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + Config.AttackDelay * _attackSpeed;
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
        public void SetUnitCoordinator(UnitCoordinator coordinator)
        {
            _brain._coordinator = coordinator;
        }
        public void ChangeAttackSpeed(float speed)
        {
            _attackSpeed *= speed;
        }
        public void ChangeMoveSpeed(float speed)
        {
            _moveSpeed *= speed;
        }
        public void ResetMoveSpeed()
        {
            _moveSpeed = 1f;
        }
        public void ResetAttackSpeed()
        {
            _attackSpeed = 1f;
        }
        public void DoubleShotOn()
        {
            _doubleShot = true;
        }
        public void DoubleShotOff()
        {
            _doubleShot = false;
        }
        public void IncreaseAttackRange(float multiplier)
        {
            _attackRangeModifier *= multiplier;
        }
        public void ResetAttackRange()
        {
            _attackRangeModifier = 1f;
        }
    }
}