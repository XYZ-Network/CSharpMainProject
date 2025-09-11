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
        public static  List<Unit> GroupUnits = new List<Unit>();
        public static int UnitCounter  = 0;/* Счетчик по заданию*/
        public static int NumberOfUnit { get; set; }/* номер юнита*/
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
        
        public Unit(UnitConfig config, Vector2Int startPos)
        {
            
            Config = config;
            
            Pos = startPos;
            Health = config.MaxHealth;
            _brain = UnitBrainProvider.GetBrain(config);
            _brain.SetUnit(this);
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
            /*unitCounter = UnitCounter;*/ /*инициализация счетчика в конструкторе*/
            if (config.IsPlayerUnit)
            {
                
                Unit.GroupUnits.Add(this);
                NumberOfUnit = ++UnitCounter; /*прсивоение новому юниту номера*/
                //Debug.Log(NumberOfUnit);
            }
            
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
        //    if (_runtimeModel?.RoMap == null)
        //    {
        //        Debug.LogError("Runtime model or map not initialized");
        //        return;
        //    }

            //    Vector2Int targetPos = _brain.GetNextStep();
            //    Vector2Int moveDelta = targetPos - Pos;

            //    // 1. Проверка расстояния перемещения
            //    if (moveDelta.sqrMagnitude > 2) // Если движение больше 1 клетки
            //    {
            //        // Корректируем до максимально допустимого перемещения
            //        targetPos = Pos + new Vector2Int(
            //            Mathf.Clamp(moveDelta.x, -1, 1),
            //            Mathf.Clamp(moveDelta.y, -1, 1)
            //        );
            //        Debug.LogWarning($"Adjusted move from {targetPos} to {Pos + new Vector2Int(Mathf.Clamp(moveDelta.x, -1, 1), Mathf.Clamp(moveDelta.y, -1, 1))}");
            //    }

            //    // 2. Проверка границ карты
            //    if (targetPos.x < 0 || targetPos.x >= _runtimeModel.RoMap.Width ||
            //        targetPos.y < 0 || targetPos.y >= _runtimeModel.RoMap.Height)
            //    {
            //        Debug.LogWarning($"Position {targetPos} is out of map bounds");
            //        return;
            //    }

            //    // 3. Проверка препятствий
            //    if (_runtimeModel.RoMap[targetPos] ||
            //        _runtimeModel.RoUnits.Any(u => u.Pos == targetPos && u != this))
            //    {
            //        Debug.Log($"Path blocked at {targetPos}");
            //        return;
            //    }

            //    // 4. Выполняем перемещение
            //    Pos = targetPos;
            //    Debug.Log($"Moved successfully to {Pos}");
            //}


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