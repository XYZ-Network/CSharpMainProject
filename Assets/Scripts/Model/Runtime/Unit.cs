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

        // ----- Эффективные характеристики -----
        public float MoveSpeedMultiplier => _moveSpeedMult.Total;
        public float AttackSpeedMultiplier => _attackSpeedMult.Total;
        public float ProjectileCountMultiplier => _projectileCountMult.Total;
        public float AttackRangeBonus => _attackRangeBonus.Total;

        public float EffectiveMoveDelay => Config.MoveDelay / Mathf.Max(0.01f, MoveSpeedMultiplier);
        public float EffectiveAttackDelay => Config.AttackDelay / Mathf.Max(0.01f, AttackSpeedMultiplier);
        public float EffectiveAttackRange => Config.AttackRange + AttackRangeBonus;

        public BaseUnitBrain Brain => _brain;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        // ----- Модификаторы от баффов/дебаффов -----
        private readonly StatModifiers _moveSpeedMult = new(1f, (a, b) => a * b);
        private readonly StatModifiers _attackSpeedMult = new(1f, (a, b) => a * b);
        private readonly StatModifiers _projectileCountMult = new(1f, (a, b) => a * b);
        private readonly StatModifiers _attackRangeBonus = new(0f, (a, b) => a + b);

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
                _nextMoveTime = time + EffectiveMoveDelay;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + EffectiveAttackDelay;
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

        public void ClearPendingProjectiles() => _pendingProjectiles.Clear();

        public void TakeDamage(int projectileDamage) => Health -= projectileDamage;

        // ----- Методы, которые вызывают баффы -----
        public void AddMoveSpeedModifier(object source, float multiplier) => _moveSpeedMult.Set(source, multiplier);
        public void RemoveMoveSpeedModifier(object source) => _moveSpeedMult.Remove(source);

        public void AddAttackSpeedModifier(object source, float multiplier) => _attackSpeedMult.Set(source, multiplier);
        public void RemoveAttackSpeedModifier(object source) => _attackSpeedMult.Remove(source);

        public void AddProjectileCountModifier(object source, float multiplier) => _projectileCountMult.Set(source, multiplier);
        public void RemoveProjectileCountModifier(object source) => _projectileCountMult.Remove(source);

        public void AddAttackRangeModifier(object source, float bonus) => _attackRangeBonus.Set(source, bonus);
        public void RemoveAttackRangeModifier(object source) => _attackRangeBonus.Remove(source);
    }
}