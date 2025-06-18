using System;
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
        public BuffSystem BuffSystem { get; }

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;
        
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
            BuffSystem = ServiceLocator.Get<BuffSystem>();
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            List<Buff> buffs = BuffSystem.GetActiveBuffs(this);
            
            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }
            
            if (_nextMoveTime < time)
            {
                _nextMoveTime = time + CalculateMoveSpeedDelay(buffs);
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + CalculateAttackSpeedDelay(buffs);
            }
        }

        private float CalculateMoveSpeedDelay(List<Buff> buffs)
        {
            try
            {
                float moveSpeedRelativeValue = buffs.Last(b => b.Type == Buff.BuffType.MoveSpeed).Value;
                return moveSpeedRelativeValue > 0
                    ? Config.MoveDelay / moveSpeedRelativeValue
                    : Config.MoveDelay * -moveSpeedRelativeValue;
            }
            catch (InvalidOperationException e)
            {
                return Config.MoveDelay;
            }
        }
        
        private float CalculateAttackSpeedDelay(List<Buff> buffs)
        {
            try
            {
                float attackSpeedRelativeValue = buffs.Last(b => b.Type == Buff.BuffType.AttackSpeed).Value;
                return attackSpeedRelativeValue > 0
                    ? Config.AttackDelay / attackSpeedRelativeValue
                    : Config.AttackDelay * -attackSpeedRelativeValue;
            }
            catch (InvalidOperationException e)
            {
                return Config.AttackDelay;
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
            var buffs = BuffSystem.GetActiveBuffs(this);
            if (buffs.Any(b => b.Type == Buff.BuffType.Invulnerability))
                return;

            Health -= projectileDamage;
        }
    }
}