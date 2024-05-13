using System.Collections.Generic;
using System.Linq;
using AssemblyCSharp.Assets.Scripts;
using AssemblyCSharp.Assets.Scripts.Controller;
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
        public UnitsCoordinator UnitsCoordinator { get; set; }
        public bool IsDead => Health <= 0;
        public BaseUnitPath ActivePath => _brain?.ActivePath; 
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        MoveSpeedBuff speedBuff = new MoveSpeedBuff(1.2f);
        AttackSpeedBuff attackSpeed = new AttackSpeedBuff(1.2f);


        public Unit(UnitConfig config, Vector2Int startPos, UnitsCoordinator unitsCoordinator)
        {
            Config = config;
            Pos = startPos;
            UnitsCoordinator = unitsCoordinator;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>(); //получаем RuntimeModel через ServiceLocator
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
                AddBuff(speedBuff);
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                AddBuff(attackSpeed);
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
            var targetPos = _brain.GetNextStep(); //когда юнит ходит, он спрашивает у Brain куда идти 
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

        public void UpdateNextMoveTime(float moveSpeedMultiplier)
        {
          
            _nextMoveTime = Time.time + Config.MoveDelay * moveSpeedMultiplier;
        }

        public void UpdateNextAttackTime(float attackSpeedMultiplier)
        {

            _nextAttackTime = Time.time + Config.MoveDelay * attackSpeedMultiplier;
        }

        public void AddBuff(IBuff<Unit> buff)
        {
            if (buff.CanApply(this))
            {
                buff.Apply(this);
            }
        }

        
    }
}