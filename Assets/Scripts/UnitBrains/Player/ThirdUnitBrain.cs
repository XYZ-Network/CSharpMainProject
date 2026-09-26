using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using UnitBrains.Pathfinding; // Импорт для использования AStarUnitPath

namespace UnitBrains.Player
{
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth"; // Имя юнита для идентификации

        // Параметры для реализации задержки атаки после движения
        private float _attackDelay = 1f; // Задержка перед атакой после остановки (1 секунда)
        private float _timeSinceLastMovement = 0f; // Время, прошедшее с момента последнего движения
        private bool _isMoving = false; // Флаг, указывающий движется ли юнит в данный момент

        // Список целей вне зоны досягаемости - юнит будет двигаться к ним используя A*
        List<Vector2Int> _outOfRangeTargets = new List<Vector2Int>();

        // Статический счетчик для присвоения уникальных номеров юнитам этого типа
        private static int _unitCounter = 0;
        // Уникальный номер текущего экземпляра юнита
        private int _unitNumber;
        // Максимальное количество целей для "умного" распределения между юнитами
        private const int MaxTargetsForSmartSelection = 3;

        // Конструктор класса - вызывается при создании каждого юнита
        public ThirdUnitBrain()
        {
            _unitNumber = _unitCounter; // Присваиваем текущий номер из счетчика
            _unitCounter++; // Увеличиваем счетчик для следующего юнита
        }

        // Метод генерации снарядов при атаке
        // forTarget - цель для выстрела
        // intoList - список для добавления созданных снарядов
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            // Всегда создаем 3 снаряда (максимальная огневая мощь Behemoth)
            int projectileCount = 3;

            // Цикл создания указанного количества снарядов
            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget); // Создаем снаряд, направленный в цель
                AddProjectileToList(projectile, intoList); // Добавляем снаряд в выходной список
            }
        }

        // Метод определения следующего шага движения юнита с использованием A*
        public override Vector2Int GetNextStep()
        {
            // Проверяем, есть ли цели для движения (вне зоны атаки)
            if (_outOfRangeTargets.Count > 0)
            {
                _isMoving = true; // Устанавливаем флаг движения
                Vector2Int target = _outOfRangeTargets[0]; // Берем первую цель из списка

                // Если мы уже на цели, останавливаемся
                if (unit.Pos == target)
                {
                    _isMoving = false; // Сбрасываем флаг движения
                    return unit.Pos; // Остаемся на месте
                }

                // Создаем A* путь от текущей позиции до цели
                ActivePath = new AStarUnitPath(runtimeModel, unit.Pos, target);

                // Получаем следующий шаг по вычисленному пути
                Vector2Int nextStep = ActivePath.GetNextStepFrom(unit.Pos);

                // Проверяем валидность шага (не больше 1 клетки)
                Vector2Int delta = nextStep - unit.Pos;
                if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
                {
                    // Если шаг невалидный, используем прямое движение
                    Debug.LogWarning($"ThirdUnitBrain: invalid A* step from {unit.Pos} to {nextStep}, using fallback");
                    nextStep = unit.Pos.CalcNextStepTowards(target);
                }

                // Проверяем проходимость выбранной клетки
                if (!IsCellWalkable(nextStep))
                {
                    Debug.LogWarning($"ThirdUnitBrain: cell {nextStep} not walkable, finding alternative");

                    // Ищем альтернативный шаг среди соседних клеток
                    Vector2Int[] directions = new Vector2Int[]
                    {
                        Vector2Int.up,    // Проверяем вверх
                        Vector2Int.right, // Проверяем вправо
                        Vector2Int.down,  // Проверяем вниз
                        Vector2Int.left   // Проверяем влево
                    };

                    bool foundAlternative = false; // Флаг нахождения альтернативы
                    foreach (Vector2Int direction in directions)
                    {
                        Vector2Int alternativeStep = unit.Pos + direction;
                        if (IsCellWalkable(alternativeStep)) // Если клетка проходима
                        {
                            nextStep = alternativeStep; // Используем альтернативу
                            foundAlternative = true; // Устанавливаем флаг
                            break; // Выходим из цикла
                        }
                    }

                    if (!foundAlternative) // Если альтернатива не найдена
                    {
                        Debug.LogWarning($"ThirdUnitBrain: no alternative step from {unit.Pos}, staying in place");
                        _isMoving = false; // Сбрасываем флаг движения
                        return unit.Pos; // Остаемся на месте
                    }
                }

                return nextStep; // Возвращаем валидный следующий шаг
            }

            // Если целей для движения нет, стоим на месте
            _isMoving = false; // Сбрасываем флаг движения
            return unit.Pos; // Возвращаем текущую позицию (юнит не двигается)
        }

        // Метод выбора целей для атаки с учетом задержки после движения
        protected override List<Vector2Int> SelectTargets()
        {
            // Очищаем список целей для движения
            _outOfRangeTargets.Clear();

            // Получаем все доступные цели
            List<Vector2Int> allTargets = new List<Vector2Int>(GetAllTargets());
            List<Vector2Int> result = new List<Vector2Int>(); // Результирующий список целей для атаки

            // БЛОКИРОВКА АТАКИ ВО ВРЕМЯ ДВИЖЕНИЯ
            // Если юнит движется или прошло меньше секунды после остановки
            if (_isMoving || _timeSinceLastMovement < _attackDelay)
            {
                // Если есть цели на поле
                if (allTargets.Count > 0)
                {
                    _outOfRangeTargets.AddRange(allTargets); // Добавляем все цели для движения к ним
                }
                else
                {
                    // Если целей нет, движемся к базе противника
                    int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                    Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyId];
                    _outOfRangeTargets.Add(enemyBasePos); // Добавляем базу как цель для движения
                }

                return result; // Возвращаем пустой список - атака заблокирована
            }
            // === КОНЕЦ БЛОКИРОВКИ АТАКИ ===

            // Если целей на поле нет
            if (allTargets.Count == 0)
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyId];

                if (IsTargetInRange(enemyBasePos)) // Если база в зоне досягаемости
                {
                    result.Add(enemyBasePos); // Добавляем базу как цель для атаки
                    return result;
                }
                else // Если база вне зоны досягаемости
                {
                    _outOfRangeTargets.Add(enemyBasePos); // Добавляем базу для движения к ней
                    return result;
                }
            }

            // Сортируем цели по расстоянию до нашей базы (приоритет - ближайшие)
            SortByDistanceToOwnBase(allTargets);

            // Распределяем цели между юнитами для равномерной атаки
            int targetIndex;
            if (allTargets.Count <= MaxTargetsForSmartSelection) // Если целей мало (≤ 3)
            {
                targetIndex = _unitNumber % allTargets.Count; // Распределяем по номеру юнита
            }
            else // Если целей много (> 3)
            {
                targetIndex = _unitNumber % MaxTargetsForSmartSelection; // Берем остаток от деления на 3
                if (targetIndex >= allTargets.Count) // Если индекс за пределами списка
                {
                    targetIndex = allTargets.Count - 1; // Корректируем на последний индекс
                }
            }

            // Получаем выбранную цель по индексу
            Vector2Int selectedTarget = allTargets[targetIndex];

            // Проверяем, находится ли цель в зоне досягаемости
            if (IsTargetInRange(selectedTarget))
            {
                result.Add(selectedTarget); // Добавляем для атаки
            }
            else
            {
                _outOfRangeTargets.Add(selectedTarget); // Добавляем для движения

                // Ищем первую цель в зоне досягаемости из всего списка
                foreach (Vector2Int target in allTargets)
                {
                    if (IsTargetInRange(target)) // Если цель доступна
                    {
                        result.Add(target); // Добавляем для атаки
                        break; // Выходим из цикла
                    }
                }

                // Если ни одна цель не доступна для атаки
                if (result.Count == 0)
                {
                    _outOfRangeTargets.AddRange(allTargets); // Добавляем все цели для движения
                }
            }

            return result; // Возвращаем список целей для атаки
        }

        // Метод обновления состояния юнита (вызывается каждый кадр)
        public override void Update(float deltaTime, float time)
        {
            base.Update(deltaTime, time); // Вызываем базовый метод обновления

            // Обновляем таймер после движения
            if (!_isMoving) // Если юнит не движется
            {
                _timeSinceLastMovement += Time.deltaTime; // Увеличиваем время с момента остановки
            }
            else // Если юнит движется
            {
                _timeSinceLastMovement = 0f; // Сбрасываем таймер при движении
            }
        }

        // Вспомогательный метод проверки проходимости клетки
        private bool IsCellWalkable(Vector2Int pos)
        {
            // Проверяем, что клетка в пределах карты
            if (pos.x < 0 || pos.x >= runtimeModel.RoMap.Width ||
                pos.y < 0 || pos.y >= runtimeModel.RoMap.Height)
                return false;

            // Проверяем, что клетка не занята стеной
            if (runtimeModel.RoMap[pos])
                return false;

            // Клетка проходима
            return true;
        }
    }
}