using System.Collections.Generic;
using System.Linq;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using Controller;
using BuffSystem;

namespace Model.Runtime
{
    public class Unit : IReadOnlyUnit, IModifiableUnit
    {
        public UnitConfig Config { get; }
        public Vector2Int Pos { get; private set; }
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;

        private readonly Dictionary<BuffType, float> _buffModifiers = new();

        public BaseUnitPath ActivePath => _brain?.ActivePath;
        public IReadOnlyList<BaseProjectile> PendingProjectiles => _pendingProjectiles;

        private readonly List<BaseProjectile> _pendingProjectiles = new();
        private IReadOnlyRuntimeModel _runtimeModel;
        private BaseUnitBrain _brain;
        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;

        private float _attackRange;
        private int _attackCount;
        public float AttackRange => _attackRange;
        public int AttackCount => _attackCount;

        public Unit(UnitConfig config, Vector2Int startPos, UnitCoordinator unitCoordinator)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;

            _attackRange = config.AttackRange;
            _attackCount = config.AttackCount;

            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _brain.SetCoordinator(unitCoordinator);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        }

        public void Update(float deltaTime, float time)
        {
            if (IsDead)
                return;

            _attackRange = GetAttackRange();
            _attackCount = (int)GetAttackCount();

            if (_nextBrainUpdateTime < time)
            {
                _nextBrainUpdateTime = time + Config.BrainUpdateInterval;
                _brain.Update(deltaTime, time);
            }

            if (_nextMoveTime < time)
            {
                //_nextMoveTime = time + Config.MoveDelay; // стандартный старый код
                _nextMoveTime = time + GetMoveDelay();
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                //_nextAttackTime = time + Config.AttackDelay; // стандартный старый код
                _nextAttackTime = time + GetAttackDelay();
            }
        }

        // Реализуем методы интерфейса //
        public float GetMoveDelay()
        {
            return Config.MoveDelay + (_buffModifiers.TryGetValue(BuffType.MoveSpeed, out var moveMod) ? moveMod : 0f);
        }

        public float GetAttackDelay()
        {
            return Config.AttackDelay + (_buffModifiers.TryGetValue(BuffType.AttackSpeed, out var attackMod) ? attackMod : 0f);
        }

        public float GetAttackRange()
        {
            return Config.AttackRange + (_buffModifiers.TryGetValue(BuffType.AttackRange, out var rangeMod) ? rangeMod : 0f);
        }

        public float GetAttackCount()
        {
            return Config.AttackCount + (_buffModifiers.TryGetValue(BuffType.AttackCount, out var attackCountMod) ? attackCountMod : 0f);
        }

        public void ApplyBuffModifier(BuffType type, float modifier)
        {
            _buffModifiers[type] = modifier;
        }

        public void RemoveBuffModifier(BuffType type)
        {
            _buffModifiers.Remove(type);
        }
        // Реализуем методы интерфейса //

        private bool Attack()
        {
            var projectiles = _brain.GetProjectiles();

            if (projectiles == null || projectiles.Count == 0)
                return false;

            //_pendingProjectiles.AddRange(projectiles); //старый стандартный код
            for (int i = 0; i < AttackCount; i++)
            {
                _pendingProjectiles.AddRange(projectiles);
            }

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

        public Unit FindUnitByPosition(Vector2Int position, bool isPlayerUnit)
        {
            var unitsToSearch = isPlayerUnit
                ? _runtimeModel.RoPlayerUnits
                : _runtimeModel.RoBotUnits;

            foreach (var unit in unitsToSearch)
            {
                if (unit is Unit gameUnit && gameUnit.Pos == position)
                {
                    return gameUnit;
                }
            }
            return null;
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