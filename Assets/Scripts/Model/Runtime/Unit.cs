using Model.BuffSystem;
using Model.Config;
using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Coordinator;
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
        private readonly IReadOnlyRuntimeModel _runtimeModel;
        private readonly BaseUnitBrain _brain;
        private UnitCoordinator _coordinator;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        // Базовая статистика для модификаторов
        private readonly float _baseMoveDelay;
        private readonly float _baseAttackDelay;
        private readonly float _baseBrainUpdateInterval;

        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            // Сохраняем базовые значения характеристик
            _baseMoveDelay = config.MoveDelay;
            _baseAttackDelay = config.AttackDelay;
            _baseBrainUpdateInterval = config.BrainUpdateInterval;
        }

        public void SetCoordinator(UnitCoordinator coordinator)
        {
            _coordinator = coordinator;
            if (_brain is DefaultPlayerUnitBrain defaultBrain)
            {
                defaultBrain.SetCoordinator(coordinator);
            }
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            // Обновление с учетом модификаторов
            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + GetModifiedBrainUpdateInterval();
                _brain.Update(deltaTime, time);
                _coordinator?.Update(deltaTime);
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

        #region Buff System Integration

        /// <summary>
        /// Применяет эффект баффа/дебаффа к юниту
        /// </summary>
        public void ApplyEffect(BuffType type, float modifier, float duration)
        {
            var buff = new BuffDebuff(type, modifier, duration);
            ServiceLocator.Get<BuffSystemManager>()?.ApplyBuff(this, buff);
        }

        private float GetModifiedMoveDelay()
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            float modifier = buffSystem?.GetModifier(this, BuffType.MoveSpeed) ?? 1f;
            return _baseMoveDelay / modifier;
        }

        private float GetModifiedAttackDelay()
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            float modifier = buffSystem?.GetModifier(this, BuffType.AttackSpeed) ?? 1f;
            return _baseAttackDelay / modifier;
        }

        private float GetModifiedBrainUpdateInterval()
        {
            var buffSystem = ServiceLocator.Get<BuffSystemManager>();
            float modifier = buffSystem?.GetModifier(this, BuffType.AttackSpeed) ?? 1f;
            return _baseBrainUpdateInterval / modifier;
        }

        #endregion

        #region Core Unit Logic

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
            Health = Mathf.Max(0, Health - projectileDamage);
        }

        #endregion
    }
}