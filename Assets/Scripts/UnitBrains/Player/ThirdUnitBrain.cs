using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";

        private enum UnitMode
        {
            Moving,
            Attacking,
            Switching
        }

        private UnitMode _currentMode = UnitMode.Moving;
        private float _modeSwitchTimer = 0f;
        private const float ModeSwitchDelay = 1f;
        private Vector2Int? _currentTarget = null;

        public override Vector2Int GetNextStep()
        {
            // Во время переключения режимов не двигаемся
            if (_currentMode == UnitMode.Switching)
                return unit.Pos;

            // Если в режиме атаки - стоим на месте
            if (_currentMode == UnitMode.Attacking)
                return unit.Pos;

            // В режиме движения - идем к цели
            if (_currentTarget.HasValue)
                return unit.Pos.CalcNextStepTowards(_currentTarget.Value);

            // Если цели нет - идем к базе противника
            var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            return unit.Pos.CalcNextStepTowards(enemyBase);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();

            // Во время переключения режимов не атакуем
            if (_currentMode == UnitMode.Switching)
                return result;

            // В режиме движения не атакуем
            if (_currentMode == UnitMode.Moving)
                return result;

            // В режиме атаки выбираем ближайшую цель
            var reachableTargets = GetReachableTargets();
            if (reachableTargets.Count > 0)
            {
                result.Add(reachableTargets[0]);
                _currentTarget = reachableTargets[0];
            }
            else
            {
                _currentTarget = null;
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time);

            // Обработка переключения режимов
            if (_currentMode == UnitMode.Switching)
            {
                _modeSwitchTimer += deltaTime;
                if (_modeSwitchTimer >= ModeSwitchDelay)
                {
                    _modeSwitchTimer = 0f;
                    _currentMode = _currentMode == UnitMode.Moving ? UnitMode.Attacking : UnitMode.Moving;
                }
                return;
            }

            // Проверяем, нужно ли переключить режим
            bool hasTargetsInRange = HasTargetsInRange();

            if ((_currentMode == UnitMode.Moving && hasTargetsInRange) ||
                (_currentMode == UnitMode.Attacking && !hasTargetsInRange))
            {
                _currentMode = UnitMode.Switching;
                _modeSwitchTimer = 0f;
            }
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            // В режиме движения или переключения не стреляем
            if (_currentMode != UnitMode.Attacking)
                return;

            // Создаем стандартный снаряд
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }
    }
}
