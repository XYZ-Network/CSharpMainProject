using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnitBrains.Player;
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
        public float movementSpeedModifier = 1f;
        public float attackSpeedModifier = 1f;
        public float attackRangeModifier = 1f;
        public bool IsDoubleAttackActive = false;
        private List<IBuff<Unit>> activeBuffs = new List<IBuff<Unit>>();
        private BuffSystem _buffSystem;

        public Unit(UnitConfig config, Vector2Int startPos, Coordinator singleton)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.coordinator = singleton;
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
                _nextMoveTime = time + Config.MoveDelay * movementSpeedModifier;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + Config.AttackDelay * attackSpeedModifier;
            }

            if (activeBuffs != null)
            {
                List<IBuff<Unit>> effectsToRemove = new List<IBuff<Unit>>();
                foreach (IBuff<Unit> buffs in activeBuffs)
                {
                    buffs.UpdateDuration(this, deltaTime);
                }
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


        public void SetMovementSpeedModifier(float modifier)
        {
            movementSpeedModifier = modifier;
        }

        public void SetAttackSpeedModifier(float modifier)
        {
            attackSpeedModifier = modifier;
        }

        public void ModifyAttackRange(float modifier)
        {
            attackRangeModifier = modifier;
            
        }

        public void SetDoubleAttackActive(bool isActive)
        {
            IsDoubleAttackActive = isActive;
        }
    }
}