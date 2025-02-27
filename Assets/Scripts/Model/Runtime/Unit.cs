using System;
using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using static UnityEngine.UI.CanvasScaler;

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
        public BaseUnitBrain Brain => _brain;

        private EffectSystem _effectSystem;
        public Coordinator _coordinator;

        
        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        private float _range;
        public float Range => _range;

        private float BrainMod = 1f;
        private float MoveMod = 1f;
        private float AttackMod = 1f;
        private float RangeMod = 1f;
        private bool _dubleStrike;
        public bool DubleStrike => _dubleStrike;
        public enum UnitModStats
        {
            BrainUpdate,
            Move,
            Attack,
            Range,
            DubleStrike
        }
        
        public Unit(UnitConfig config, Vector2Int startPos, Coordinator coordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _coordinator = coordinator;

            _range = Config.AttackRange * RangeMod;
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;
            _range = Config.AttackRange * RangeMod;

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + (Config.BrainUpdateInterval * BrainMod);
                _brain.Update(deltaTime, time);
            }
            
            if (_nextMoveTime < time)
            {
                _nextMoveTime = time + (Config.MoveDelay * MoveMod);
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + (Config.AttackDelay * AttackMod);
            }
        }
        public void StatModification<T>(UnitModStats stats, T modificator)
        {
            switch (stats)
            {
                case UnitModStats.Attack:
                    AttackMod = Convert.ToSingle(modificator);

                    break;
                case UnitModStats.Move:
                    MoveMod = Convert.ToSingle(modificator);
                    break;
                case UnitModStats.BrainUpdate:
                    BrainMod = Convert.ToSingle(modificator);
                    break;
                case UnitModStats.Range:
                    RangeMod = Convert.ToSingle(modificator);
                    break;
                case UnitModStats.DubleStrike:
                    _dubleStrike = Convert.ToBoolean(modificator);
                    break;
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