using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnitBrains;
using UnityEngine;
using Utilities;
using System;
using BuffDebuff;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public BaseUnitBrain Brain { get; private set; }
        public BaseUnitPath ActivePath => Brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        public int PlayerId => throw new System.NotImplementedException();

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private readonly List<BuffDebuff.BuffDebuff> _buffs = new();
        private IReadOnlyRuntimeModel _runtimeModel;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        private float _attackRange;

        private bool _isDoubleShotEnabled;

        public void EnableDoubleShot()
        {
            _isDoubleShotEnabled = true;
        }

        public void IncreaseAttackRange(float amount)
        {
            _attackRange += amount;
        }

        public float AttackRange => _attackRange;
        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            Brain = UnitBrainProvider.GetBrain(config);
            Brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        private List<BuffDebuff.BuffDebuff> _buffsDebuffs = new List<BuffDebuff.BuffDebuff>();

        public bool HasBuff(string buffName)
        {
            return _buffsDebuffs.Exists(b => b.Name == buffName);
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                Brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                _nextMoveTime = time + GetMoveDelay();
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + GetAttackDelay();
            }

            UpdateBuffs(deltaTime);
        }

        private void UpdateBuffs(float deltaTime)
        {
            foreach (var buff in _buffs.ToList())
            {
                buff.Duration -= deltaTime;
                if (buff.Duration <= 0)
                {
                    _buffs.Remove(buff);
                }
            }
        }

        private float GetMoveDelay()
        {
            float moveDelay = Config.MoveDelay;
            foreach (var buff in _buffs)
            {
                moveDelay *= buff.MoveSpeedModifier;
            }
            return moveDelay;
        }

        private float GetAttackDelay()
        {
            float attackDelay = Config.AttackDelay;
            foreach (var buff in _buffs)
            {
                attackDelay *= buff.AttackSpeedModifier;
            }
            return attackDelay;
        }

        private bool Attack()
        {
            var projectiles = Brain.GetProjectiles();
            if (projectiles == null || projectiles.Count == 0)
                return false;

            _pendingProjectiles.AddRange(projectiles);
            return true;
        }

        private void Move()
        {
            var targetPos = Brain.GetNextStep();
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

        public void ApplyBuff(BuffDebuff.BuffDebuff buff)
        {
            _buffs.Add(buff);
        }

        public bool HasBuff(Type buffType)
        {
            throw new NotImplementedException();
        }
    }
}

