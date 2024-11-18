using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public static int IdBilling = 0;
        public int Id;

        public override string TargetUnitName => "Ironclad Behemoth";

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int MaxTarget = 3;
        private const float TransitionTime = 1f; // Время перехода между действиями

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private bool _isTransitioning = false; // Флаг перехода.
        private bool _isMoving = false; // Текущее состояние (движение или атака)


        private Vector2Int _notRangeEnemyPosition;
        private List<Vector2Int> allTargetEnemies = new List<Vector2Int>();

        public ThirdUnitBrain()
        {
            Id = IdBilling++;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            allTargetEnemies.Clear();
            allTargetEnemies = GetAllTargets().ToList();

            Vector2Int targetBaseEnemy = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            allTargetEnemies.Remove(targetBaseEnemy);

            // Если нужно оставить только одну цель
            if (allTargetEnemies.Count > 0)
            {
                SortByDistanceToOwnBase(allTargetEnemies);

                if (allTargetEnemies.Count > MaxTarget)
                {
                    allTargetEnemies = allTargetEnemies.GetRange(0, MaxTarget);
                }

                int idEnemy = (Id - 1) % allTargetEnemies.Count;
                Vector2Int target = allTargetEnemies[idEnemy];
                allTargetEnemies.Clear();

                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = target;
                }
                else
                {
                    allTargetEnemies.Add(target);
                }
            }
            else
            {
                allTargetEnemies.Clear();

                if (!HasTargetsInRange())
                {
                    _notRangeEnemyPosition = targetBaseEnemy;
                }
                else
                {

                    allTargetEnemies.Add(targetBaseEnemy);
                }
            }
            return allTargetEnemies;
        }

        public override Vector2Int GetNextStep()
        {
            // Возвращаем следующий шаг к цели
            return unit.Pos.CalcNextStepTowards(_notRangeEnemyPosition);
        }

        public override void Update(float deltaTime, float time)
        {
            _cooldownTime += deltaTime;

            if (_isTransitioning)
            {
                if (_cooldownTime >= TransitionTime)
                {
                    _isTransitioning = false;
                    _cooldownTime = 0f; // Сброс таймера
                    _isMoving = !_isMoving; // Переключение состояния
                }
                return; // Во время перехода ничего не делаем
            }

            if (_overheated)
            {
                _cooldownTime += deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);

                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }

            if (_isMoving)
            {
                if (CanAttack())
                {
                    StartTransition(); // Начать переход в атаку
                }
                else
                {
                    Move();
                }
            }
            else
            {
                Attack(); // Выполняем атаку
            }
        }

        private void StartTransition()
        {
            _isTransitioning = true;
            _cooldownTime = 0f; // Сброс таймера
        }

        void Move()
        
        {
            Vector2Int nextStep = GetNextStep();
            // Логика для движения к цели
        }

        private void Attack()
        {
            var projectiles = GetProjectiles();
            if (projectiles != null && projectiles.Count > 0)
            {
                unit.ClearPendingProjectiles(); 

                
                List<BaseProjectile> newProjectiles = new List<BaseProjectile>(projectiles);

                
                foreach (var projectile in newProjectiles)
                {
                    // Добавляем по одному
                    ((List<BaseProjectile>)unit.PendingProjectiles).Add(projectile);
                }

                IncreaseTemperature(); // Увеличиваем температуру после стрельбы
                StartTransition(); // Начать переход в движение
            }
        }


        private bool CanAttack()
        {
                return !(_overheated || !HasTargetsInRange());
        }

        

        private int GetTemperature()
        {
            return _overheated ? (int)OverheatTemperature : (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}
