using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.BuffsAndDebuffs;
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
        private BaseUnitBrain _brain;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        private BuffAndDebuffControllSystem _buffAndDebuffControllSystem;

        public Unit(UnitConfig config, Vector2Int startPos, PathAndTargetCoordinator pathAndTargetCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetControler(pathAndTargetCoordinator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _buffAndDebuffControllSystem = ServiceLocator.Get<BuffAndDebuffControllSystem>();
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
                var actualModifier = _buffAndDebuffControllSystem.GetActualModifier(this);
                _nextMoveTime = time + Config.MoveDelay/actualModifier.moveMod;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                var actualModifier = _buffAndDebuffControllSystem.GetActualModifier(this);

                _nextAttackTime = time + Config.AttackDelay/actualModifier.attackMod;
            }

           
                if (Input.GetKey(KeyCode.Q))
                {
                    _buffAndDebuffControllSystem.AddItem(this, new MovementBuff(this));
                }
                else if (Input.GetKey(KeyCode.W))
                {
                    _buffAndDebuffControllSystem.AddItem(this, new AttackBuff(this));
                }

            if (Input.GetKey(KeyCode.A))
                {
                     _buffAndDebuffControllSystem.RemoveItem(this, new MovementDebuff(this));
                }
            else if (Input.GetKey(KeyCode.S))
                {
                     _buffAndDebuffControllSystem.RemoveItem(this, new AttackDebuff(this));
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