using System.Collections.Generic;
using System.Linq;
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
        protected EffectController _effects;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        private bool _takeEffectOnce = true; //Эта строчка добовляет эффект в начале(потом удалить)

        public UnitsCoordinator Coordinator;
        public Unit(UnitConfig config, Vector2Int startPos, UnitsCoordinator _coordinator)
        {
            Coordinator = _coordinator;
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _effects = ServiceLocator.Get<EffectController>();
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;
            if (_takeEffectOnce) //Эта строчка добовляет эффект в начале(потом удалить)
            {
                _effects.AddEffect(this, EffectController.TypesOfEffects.DelayForNextMoveTimeEffect, 5, 5);
                _takeEffectOnce=false;
            }
            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }
            
            if (_nextMoveTime < time)
            {
                _nextMoveTime = time + Config.MoveDelay * _effects.FindEffectMultiplier(this, EffectController.TypesOfEffects.DelayForNextMoveTimeEffect);
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + Config.AttackDelay * _effects.FindEffectMultiplier(this,EffectController.TypesOfEffects.DelayForNextAttackTimeEffect);
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