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
    public class Unit : IReadOnlyUnit, IStatsDynamic
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

        private float _attackDelayDynamic = 0f;
        private float _attackRangeExpandDynamic = 0f;
        private int _multiplierShotDynamic = 1;

        public float AttackDelayStat => Config.AttackDelay + _attackDelayDynamic;
        public float AttackRangeStat => Config.AttackRange + _attackRangeExpandDynamic;
        public int MultiplierShotStat => _multiplierShotDynamic;


        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private static int _uid = 0;
        public int UnitId { get; private set; }

        public Unit(UnitConfig config, Vector2Int startPos, GroupBrain groupBrain)
        {
            UnitId = _uid++;
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetGroupBrain(groupBrain);
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
                _nextMoveTime
                    = time + Config.MoveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime
                    = time + AttackDelayStat;
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

        public void ChangeDelayAttack(float modifier)
        {
            _attackDelayDynamic += modifier;
        }

        public void ChangeMultiplierShot(int modifier)
        {
            _multiplierShotDynamic += modifier;

            if (_multiplierShotDynamic < 1)
                _multiplierShotDynamic = 1;
        }
        public void ChangeAttackRange(float modifier)
        {
            _attackRangeExpandDynamic += modifier;
        }

        public string GetName()
        {
            return Config.Name;
        }
    }
}