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
        public UnitConfig Config { get; private set; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        public float MoveCoef = 0;
        public float AtcCoef = 0;
        public float AtcRange = 0;
        public float sing_or_duble_bullet = 1;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        
        private SystemForBuffs _buffs = ServiceLocator.Get<SystemForBuffs>();
        public Unit(UnitConfig config, Vector2Int startPos, SingleThing thing)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this, thing);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;
            //sing_or_duble_bullet = _buffs.Push(this, new DoubleAttack<Unit>(2f, 2f));
            _buffs.Push(this, new DoubleAttack<Unit>(3,3));
            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }
            _buffs.Push(this, new MoreRange<Unit>(3f, 5f));

            if (_nextMoveTime < time)
            {
                MoveCoef = _buffs.IfTrueUnit(this, "SpdBuff");
                _nextMoveTime = time + Config.MoveDelay - MoveCoef;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                AtcCoef = _buffs.Push(this, new BufAt<Unit>(4f, 0.65f));
                _nextAttackTime = time + (Config.AttackDelay - AtcCoef);
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