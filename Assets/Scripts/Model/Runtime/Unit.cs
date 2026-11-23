using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Buffs;
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
        
        // Actual values that can be modified by buffs
        public float CurrentMoveDelay { get; private set; }
        public float CurrentAttackDelay { get; private set; }
        public float CurrentAttackRange { get; private set; }
        public int ProjectilesPerShot { get; private set; }
        public bool Invulnerability { get; private set; }
        
        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        
        public Unit(UnitConfig config, UnitCoordinator coordinator, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetCoordinator(coordinator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            CurrentMoveDelay = Config.MoveDelay;
            CurrentAttackDelay = Config.AttackDelay;
            CurrentAttackRange = Config.AttackRange;
            ProjectilesPerShot = 1;
            Invulnerability = false;
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
                _nextMoveTime = time + CurrentMoveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + CurrentAttackDelay;
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
            if (Invulnerability) 
                return;

            Health -= projectileDamage;
        }
        
        public void ModifyMoveSpeed(float value) =>
            CurrentMoveDelay = value > 0
                ? Config.MoveDelay / value
                : Config.MoveDelay * -value;
        
        public void ResetMoveSpeed() =>
            CurrentMoveDelay = Config.MoveDelay;
        
        public void ModifyAttackSpeed(float value) =>
            CurrentAttackDelay = value > 0
                ? Config.AttackDelay / value
                : Config.AttackDelay * -value;
        
        public void ResetAttackSpeed() =>
            CurrentAttackDelay = Config.AttackDelay;

        public void ModifyAttackRange(int value) =>
            CurrentAttackRange += value;

        public void ResetAttackRange() =>
            CurrentAttackRange = Config.AttackRange;

        public void ModifyProjectilesPerShot(int value) => 
            ProjectilesPerShot += value;

        public void ResetProjectilesPerShot() =>
            ProjectilesPerShot = Config.ProjectilesPerShot;

        public void SetInvulnerability(bool value) =>
            Invulnerability = value;
    }
}