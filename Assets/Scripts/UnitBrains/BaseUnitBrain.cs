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
        public virtual string TargetUnitName => string.Empty; // Имя целевого юнита для идентификации
        public virtual bool IsPlayerUnitBrain => true; // Флаг, указывающий принадлежность к игроку

        // Свойство ActivePath с публичным геттером и защищенным сеттером
        // Позволяет наследникам устанавливать активный путь
        public virtual BaseUnitPath ActivePath
        {
            get => _activePath; // Публичный геттер для чтения пути
            protected set => _activePath = value; // Защищенный сеттер для установки из наследников
        }

        protected Unit unit { get; private set; } // Ссылка на управляемый юнит
        protected IReadOnlyRuntimeModel runtimeModel => ServiceLocator.Get<IReadOnlyRuntimeModel>(); // Доступ к модели игры
        private BaseUnitPath _activePath = null; // Приватное поле для хранения активного пути

        // Массив смещений для визуального разброса снарядов при атаке
        private readonly Vector2[] _projectileShifts = new Vector2[]
        {
            new (0f, 0f),      // Центральный снаряд
            new (0.15f, 0f),   // Смещение вправо
            new (0f, 0.15f),   // Смещение вверх
            new (0.15f, 0.15f), // Смещение вправо-вверх
            new (0.15f, -0.15f), // Смещение вправо-вниз
            new (-0.15f, 0.15f), // Смещение влево-вверх
            new (-0.15f, -0.15f), // Смещение влево-вниз
        };

        // Виртуальный метод получения следующего шага движения
        public virtual Vector2Int GetNextStep()
        {
            // Если есть цели в зоне атаки, стоим на месте и атакуем
            if (HasTargetsInRange())
                return unit.Pos; // Возвращаем текущую позицию (не двигаемся)

            // Получаем позицию вражеской базы как цель по умолчанию
            var target = runtimeModel.RoMap.Bases[
                IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            // Создаем A* путь от текущей позиции до базы врага
            _activePath = new AStarUnitPath(runtimeModel, unit.Pos, target);

            // Получаем следующий шаг по вычисленному пути
            Vector2Int nextStep = _activePath.GetNextStepFrom(unit.Pos);

            // Проверяем валидность шага (не больше 1 клетки в любом направлении)
            Vector2Int delta = nextStep - unit.Pos;

            // Если шаг невалидный (слишком большой), используем прямое движение к цели
            if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
            {
                Debug.LogWarning($"A* returned invalid step {nextStep} from {unit.Pos}, using fallback movement");
                nextStep = unit.Pos.CalcNextStepTowards(target); // Вычисляем прямой шаг к цели
            }

            // Дополнительная проверка: можно ли пройти в выбранную клетку
            if (!IsCellWalkable(nextStep))
            {
                Debug.LogWarning($"Cannot walk to {nextStep} from {unit.Pos}, finding alternative");
                nextStep = FindNearbyWalkableCell(unit.Pos); // Ищем альтернативную проходимую клетку
            }

            // Финальная проверка валидности шага
            delta = nextStep - unit.Pos;
            if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
            {
                Debug.LogError($"Unable to find valid step from {unit.Pos}, staying in place");
                return unit.Pos; // Остаемся на месте если ничего не помогло
            }

            return nextStep; // Возвращаем валидный следующий шаг
        }

        // Метод получения снарядов для атаки
        public List<BaseProjectile> GetProjectiles()
        {
            List<BaseProjectile> result = new(); // Создаем список для снарядов

            // Для каждой выбранной цели генерируем снаряды
            foreach (var target in SelectTargets())
            {
                GenerateProjectiles(target, result); // Генерируем снаряды для цели
            }

            // Добавляем визуальные смещения снарядам для реалистичности стрельбы
            for (int i = 0; i < result.Count; i++)
            {
                var proj = result[i]; // Получаем снаряд
                proj.AddStartShift(_projectileShifts[i % _projectileShifts.Length]); // Добавляем смещение
            }

            return result; // Возвращаем список готовых снарядов
        }

        // Метод установки ссылки на управляемый юнит
        public void SetUnit(Unit unit)
        {
            this.unit = unit; // Сохраняем ссылку на юнит
        }

        // Виртуальный метод обновления (вызывается каждый кадр)
        public virtual void Update(float deltaTime, float time)
        {
            // Базовая реализация пустая, переопределяется в наследниках при необходимости
        }

        // Виртуальный метод генерации снарядов для атаки
        protected virtual void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            // Создаем один снаряд и добавляем его в список
            AddProjectileToList(CreateProjectile(forTarget), intoList);
        }

        // Виртуальный метод выбора целей для атаки
        protected virtual List<Vector2Int> SelectTargets()
        {
            var result = GetReachableTargets(); // Получаем все цели в зоне досягаемости

            // Оставляем только одну цель (первую в списке) по умолчанию
            while (result.Count > 1)
                result.RemoveAt(result.Count - 1); // Удаляем лишние цели

            return result; // Возвращаем список с одной целью
        }

        // Метод создания снаряда указанного типа
        protected BaseProjectile CreateProjectile(Vector2Int target) =>
            BaseProjectile.Create(unit.Config.ProjectileType, unit, unit.Pos, target, unit.Config.Damage);

        // Метод добавления снаряда в список
        protected void AddProjectileToList(BaseProjectile projectile, List<BaseProjectile> list) =>
            list.Add(projectile); // Добавляем снаряд в переданный список

        // Метод получения юнита на указанной позиции
        protected IReadOnlyUnit GetUnitAt(Vector2Int pos) =>
            runtimeModel.RoUnits.FirstOrDefault(u => u.Pos == pos); // Ищем юнита по позиции

        // Метод получения юнитов в указанном радиусе
        protected List<IReadOnlyUnit> GetUnitsInRadius(float radius, bool enemies)
        {
            var units = new List<IReadOnlyUnit>(); // Создаем пустой список для результатов
            var pos = unit.Pos; // Текущая позиция нашего юнита
            var distanceSqr = radius * radius; // Квадрат радиуса для оптимизации вычислений

            // Проверяем всех юнитов в игре
            foreach (var otherUnit in runtimeModel.RoUnits)
            {
                if (otherUnit == unit) // Пропускаем самого себя
                    continue;

                // Проверяем соответствие фильтру (враги или союзники)
                if (enemies != (otherUnit.Config.IsPlayerUnit == unit.Config.IsPlayerUnit))
                    continue;

                var otherPos = otherUnit.Pos; // Позиция проверяемого юнита
                var diff = otherPos - pos; // Разница позиций
                var distance = diff.sqrMagnitude; // Квадрат расстояния между юнитами

                if (distance <= distanceSqr) // Если юнит в заданном радиусе
                    units.Add(otherUnit); // Добавляем его в список
            }

            return units; // Возвращаем список найденных юнитов
        }

        // Метод проверки наличия целей в зоне атаки
        protected bool HasTargetsInRange()
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange; // Квадрат радиуса атаки

            // Проверяем все возможные цели
            foreach (var possibleTarget in GetAllTargets())
            {
                var diff = possibleTarget - unit.Pos; // Разница позиций с целью
                if (diff.sqrMagnitude < attackRangeSqr) // Если цель в радиусе атаки
                    return true; // Цель найдена в зоне досягаемости
            }

            return false; // Целей в зоне атаки нет
        }

        // Метод получения всех вражеских юнитов
        protected IEnumerable<IReadOnlyUnit> GetAllEnemyUnits()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain); // Фильтруем юниты по принадлежности
        }

        // Метод получения всех возможных целей (вражеские юниты + база)
        protected IEnumerable<Vector2Int> GetAllTargets()
        {
            return runtimeModel.RoUnits
                .Where(u => u.Config.IsPlayerUnit != IsPlayerUnitBrain) // Все вражеские юниты
                .Select(u => u.Pos) // Получаем их позиции
                .Append(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]); // Добавляем вражескую базу
        }

        // Метод проверки, находится ли цель в зоне атаки
        protected bool IsTargetInRange(Vector2Int targetPos)
        {
            var attackRangeSqr = unit.Config.AttackRange * unit.Config.AttackRange; // Квадрат радиуса атаки
            var diff = targetPos - unit.Pos; // Разница позиций
            return diff.sqrMagnitude <= attackRangeSqr; // Возвращаем true если цель в радиусе
        }

        // Метод получения всех целей в зоне досягаемости атаки
        protected List<Vector2Int> GetReachableTargets()
        {
            var result = new List<Vector2Int>(); // Создаем пустой список результатов

            // Проверяем все возможные цели
            foreach (var possibleTarget in GetAllTargets())
            {
                if (!IsTargetInRange(possibleTarget)) // Если цель не в зоне атаки
                    continue; // Пропускаем её

                result.Add(possibleTarget); // Добавляем доступную цель в список
            }

            return result; // Возвращаем список доступных целей
        }

        // Вспомогательный метод проверки проходимости клетки
        private bool IsCellWalkable(Vector2Int pos)
        {
            // Проверяем, что клетка находится в пределах игровой карты
            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width ||
                pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                return false; // Клетка за пределами карты

            // Проверяем, что клетка не занята препятствием (стеной)
            if (runtimeModel.RoMap[pos])
                return false; // Клетка заблокирована

            // Проверяем, что на клетке нет другого юнита
            if (runtimeModel.RoUnits.Any(u => u.Pos == pos))
                return false; // Клетка занята юнитом

            return true; // Клетка проходима
        }

        // Поиск ближайшей проходимой клетки рядом с текущей позицией
        private Vector2Int FindNearbyWalkableCell(Vector2Int currentPos)
        {
            // Определяем приоритетные направления для поиска
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,    // Вверх
                Vector2Int.right, // Вправо
                Vector2Int.down,  // Вниз
                Vector2Int.left   // Влево
            };

            // Проверяем каждое направление
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = currentPos + direction; // Вычисляем позицию соседа

                // Если соседняя клетка проходима, возвращаем её
                if (IsCellWalkable(neighborPos))
                {
                    return neighborPos; // Найдена проходимая клетка
                }
            }

            // Если все соседние клетки заняты, остаемся на месте
            Debug.LogWarning($"No walkable cells near {currentPos}, staying in place");
            return currentPos; // Возвращаем текущую позицию
        }
    }
}