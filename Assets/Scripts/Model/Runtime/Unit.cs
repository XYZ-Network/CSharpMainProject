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

        public float AttackDelay { get; private set; }
        public float MoveDelay { get; private set; }
        public float AttackRange { get; private set; }
        public bool IsBuffed { get; private set; } = false;
        public bool DoubleAttack { get; private set; } = false;


        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        public UnitCoordinator UnitCoordinator;

        private EffectsForUnits _effects;//

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();


            AttackDelay = config.AttackDelay;//
            MoveDelay = config.MoveDelay; //
            AttackRange = config.AttackRange; //


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
                _nextMoveTime = time + MoveDelay;  // изначально Config.MoveDelay
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + AttackDelay;   // изначально Config.AttackDelay
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

        //
        //
        //

        public void ChangeAttackDelay(float attackDelay)
        {
            AttackDelay = attackDelay;
        }


        public void ChangeMoveDelay(float moveDelay)
        {
            MoveDelay = moveDelay;
        }

        public void ChangeAttackRange(float attackRange)
        {
            AttackRange = attackRange;
        }

        public void ChangeDoubleAttackStatus(bool status)
        {
            DoubleAttack = status;
        }

        public void ChangeIsBuffedStatus(bool status)
        {
            IsBuffed = status;
        }

        public string GiveUnitName()
        {
            return _brain.TargetUnitName;
        }

        //public enum UnitType
        //{ 
        // First,
        // Second,
        // Third,
        // Fouth,
        // Other
        //}

    }
}