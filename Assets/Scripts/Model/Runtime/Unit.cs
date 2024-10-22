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
        public string UnitID { get; private set; }
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

        private BuffService _buffService => ServiceLocator.Get<BuffService>();
        private bool isBuffable = false;
                
        public Unit(UnitConfig config, Vector2Int startPos, RecommendationsForUnitsSingleton recommendationsForUnitsSingleton) // 4. добавлена зависимость
        {
            UnitID = IDGenerator.GenerateStringID(10);
            Config = config;
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this, recommendationsForUnitsSingleton); // 5.добавлена зависимость
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

                if(_buffService._buffs.ContainsKey(this.UnitID))
                {
                    _nextMoveTime = time + _buffService._buffs[UnitID].moveSpeed;
                } else
                {
                    _nextMoveTime = time + Config.MoveDelay;
                }
                              
                Move();
            }
            
            if (_nextAttackTime < time && Attack())
            {
                if (_buffService._buffs.ContainsKey(this.UnitID))
                {
                    _nextAttackTime = time + _buffService._buffs[UnitID].shootSpeed;
                } else
                {
                    _nextAttackTime = time + Config.AttackDelay;
                }
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

        public void TakeBuff()
        {
            if(!isBuffable)
            _buffService.TempBuff(UnitID, _buffService.GetRandomBuff(), 5f);

            isBuffable = true;
        }
    }
}