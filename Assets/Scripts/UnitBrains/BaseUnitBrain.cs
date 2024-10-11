using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using Model.Runtime.ReadOnly;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using Unit = Model.Runtime.Unit;

namespace UnitBrains
{
    public abstract class BaseUnitBrain
    {
       
        public virtual string TargetUnitName => string.Empty;

        public virtual bool IsPlayerUnitBrain => true;

        // Активный путь к цели
        public virtual BaseUnitPath ActivePath => _activePath;

        protected Unit unit { get; private set; } // Юнит, которому принадлежит мозг
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>(); // Модель игры
        private BaseUnitPath _activePath = null; // Активный путь для юнита

        // Смещения для проекций с учетом запуска
        private readonly Vector2[] _projectileShifts = new Vector2[]
        {
            new (0f, 0f),
            new (0.15f, 0f),
            new (0f, 0.15f),
            new (0.15f, 0.15f),
            new (0.15f, -0.15f),
            new (-0.15f, 0.15f),
            new (-0.15f, -0.15f),
        };

        // Получение следующего шага юнита
        public virtual Vector2Int GetNextStep()
        {
            if (HasTargetsInRange())
                return unit.Pos; // Если есть цель в радиусе, возвращаем текущую позицию юнита

            // Определение целевой базы
            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            // Создание пути к цели
            _activePath = new SmartUnitPath(runtimeModel, unit.Pos, target);

            // Получение следующего шага по пути
            return _activePath.GetNextStepFrom(unit.Pos);
        }

        // Получение всех проекций для атаки
        public List<BaseProjectile> GetProjectiles()
        {
            List<BaseProjectile> result = new();
            // Генерация проекций для выбранных целей
            foreach (var target in SelectTargets())
            {
                GenerateProjectiles(target, result);
            }

            // Добавление смещений к каждой проекции
            for (int i = 0; i < result.Count; i++)
            {
                var proj = result[i];
                proj.AddStartShift(_projectileShifts[i % _projectileShifts.Length]);
            }

            return result;
        }

        // Установка юнита
        public void SetUnit(Unit unit)
        {
            this.unit = unit;
        }

        // Обновление состояния юнита
        public virtual void Update(float deltaTime, float time)
        {
        }

        // Генерация проекций для заданной цели
        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        }

        // Выбор целей для атаки
        protected virtual List<Vector2Int> SelectTargets()
        {
            var result = GetReachableTargets(); // Получение всех доступных целей
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1); // Убираем лишние цели (оставляем одну)
            return result;
        }

        // Создание проекции для атаки
        protected BaseProjectile CreateProjectile(Vector2Int target) =>
            BaseProjectile.Create(unit.Config.ProjectileType, unit, unit.Pos, target, unit.Config.Damage);

        // Добавление проекции в список
        protected void AddProjectileToList(BaseProjectile projectile, List<BaseProjectile> list) =>
            list.Add(projectile);

        // Получение юнита по позиции
        protected IReadOnlyUnit GetUnitAt(Vector2Int pos) =>
            runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == pos);

        // Получение всех юнитов в радиусе
        protected List<IReadOnlyUnit> GetUnitsInRadius(float radius, bool enemies)
        {
            var units = new List<IReadOnlyUnit>();
            var pos = unit.Pos;
            var distanceSqr = radius * radius; // Сравнение по квадрату расстояния для (повышения производительности)

            foreach (var otherUnit in runtimeModel.RoUnits)
            {
                if (otherUnit == unit) continue; // Пропуск самого юнита

                if (enemies != (otherUnit.Config.IsPlayerUnit == unit.Config.IsPlayerUnit)) continue; // Проверка на врагов

                var otherPos = otherUnit.Pos;
                var diff = otherPos - pos;
                var distance = diff.sqrMagnitude; // Рассчет расстояния
                if (distance <= distanceSqr)
                    units.Add(otherUnit); // Добавление юнита в список
            }

            return units;
        }

        // Проверка наличия целей в диапазоне атаки
        protected bool HasTargetsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange; // Квадрат радиуса атаки
            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos;
                if (diff.sqrMagnitude < attackRangeSqr)
                    return true; // Если цель в диапазоне атаки
            }

            return false;
        }

        // Получение всех вражеских юнитов
        protected IEnumerable<IReadOnlyUnit> GetAllEnemyUnits()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain);
        }

        // Получение всех целей
        protected IEnumerable<Vector2Int> GetAllTargets()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain)
                .Select(u => u.Pos)
                .Append(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
        }

        // Проверка, находится ли цель в диапазоне атаки
        protected bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            var diff = targetPos - unit.Pos;
            return diff.sqrMagnitude <= attackRangeSqr; // Проверка по квадрату расстояния
        }

        // Получение всех доступных целей
        protected List<Vector2Int> GetReachableTargets()
        {
            var result = new List<Vector2Int>();
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange;
            foreach (var possibleTarget in GetAllTargets())
            {
                if (!IsTargetInRange(possibleTarget))
                    continue; // Пропуск цели, если она вне диапазона

                result.Add(possibleTarget); // Добавление доступной цели
            }

            return result;
        }
    }
}
