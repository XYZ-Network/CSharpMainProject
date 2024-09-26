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
        private BaseUnitBrain _brain;
        private BuffSystem _buffSystem;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        public Unit(UnitConfig config, Vector2Int startPos, UnitCoordinator unitCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetCoordinator(unitCoordinator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _buffSystem = ServiceLocator.Get<BuffSystem>();

            int randomNumber = Random.Range(1, 6);
            switch (randomNumber)
            {
                case 1:
                    _buffSystem.setBuff(this, new UpMoveSpeedBuff());
                    break;
                case 2:
                    _buffSystem.setBuff(this, new DownMoveSpeedBuff());
                    break;
                case 3:
                    _buffSystem.setBuff(this,new UpAttackSpeedBuff());
                    break;
                case 4:
                    _buffSystem.setBuff(this, new DownAttackSpeedBuff());
                    break;
                case 5:
                    // no buff for unit
                    break;
            }
        }

        public void Update(float deltaTime, float time)
        {
            _buffSystem.UpdateBuffDuration();

            if (IsDead)
                return;

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                BuffNames[] moveBuffs = { BuffNames.UpMoveSpeed, BuffNames.DownMoveSpeed };
                AbstractBuff[] buffs = this._buffSystem.getBuffs(this, moveBuffs);
                float modifier = (buffs != null) ? buffs.Aggregate(1.0f, (current, buff) => current * buff.Modifier) : 1;

                _nextMoveTime = (time + Config.MoveDelay) * modifier;
                Move();
            }

            if (_nextAttackTime < time && Attack())
            {
                BuffNames[] moveBuffs = { BuffNames.UpAttackSpeed, BuffNames.DownAttackSpeed };
                AbstractBuff[] buffs = this._buffSystem.getBuffs(this, moveBuffs);
                float modifier = (buffs != null) ? buffs.Aggregate(1.0f, (current, buff) => current * buff.Modifier) : 1;

                _nextAttackTime = (time + Config.AttackDelay) * modifier;
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