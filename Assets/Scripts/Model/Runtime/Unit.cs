using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
using Model.Config;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;
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

        private readonly TimeUtil _timeUtil;

        private float _nextBrainUpdateTime = 0f;
        private float _nextMoveTime = 0f;
        private float _nextAttackTime = 0f;
        
        public Unit(UnitConfig config, Vector2Int startPos)
        {
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
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
                _nextMoveTime = time + Config.MoveDelay;
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                _nextAttackTime = time + Config.AttackDelay;
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

    public class Coordinator
    {
        private static Coordinator _instance;
        private IReadOnlyRuntimeModel _runtimeModel;
        private TimeUtil _timeUtil;

        private Coordinator()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            _timeUtil = ServiceLocator.Get<TimeUtil>();
        }

        public static Coordinator GetInstance()
        {
            if (_instance == null)
                _instance = new Coordinator();

            return _instance;
        }

        public Vector2Int SuggestedAttackTarget()
        {
            Vector2Int? resultByDistance = null;
            Vector2Int? resultByHealth = null;
            float minDistance = float.MaxValue;
            float minHealth = float.MaxValue;

            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            foreach (var enemy in _runtimeModel.RoBotUnits)
            {
                if (enemy.Pos.y < _runtimeModel.RoMap.Height / 2)
                {
                    float distance = Vector2Int.Distance(enemy.Pos, playerBase);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        resultByDistance = enemy.Pos;
                    }
                }
                else
                {
                    float health = enemy.Health;
                    if (enemy.Health < minHealth)
                    {
                        minHealth = enemy.Health;
                        resultByHealth = enemy.Pos;
                    }
                }
            }

            if (resultByDistance != null)
                return (Vector2Int)resultByDistance;
            else if (resultByHealth != null)
                return (Vector2Int)resultByHealth;
            else
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        }

        public Vector2Int SuggestedMoveTarget()
        {
            Vector2Int? result = null;
            float minDistance = float.MaxValue;
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            foreach (var enemy in _runtimeModel.RoBotUnits)
            {
                if (enemy.Pos.y < _runtimeModel.RoMap.Height / 2)
                {
                    return playerBase;
                }
                else
                {
                    float distance = Vector2Int.Distance(enemy.Pos, playerBase);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = enemy.Pos;
                    }
                }
            }

            if (result != null)
                return (Vector2Int)result;
            else
                return _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
        }
    }
}