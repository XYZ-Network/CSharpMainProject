using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using UnitBrains.Pathfinding; // Импорт для использования AStarUnitPath

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando"; // Имя юнита для идентификации
        private const float OverheatTemperature = 3f; // Пороговая температура перегрева оружия
        private const float OverheatCooldown = 2f; // Время остывания после перегрева (в секундах)
        private float _temperature = 0f; // Текущая температура оружия
        private float _cooldownTime = 0f; // Время, прошедшее с начала остывания
        private bool _overheated; // Флаг, указывающий на перегрев оружия

        // Список целей, находящихся вне зоны досягаемости атаки
        // Юнит будет двигаться к этим целям, используя A* pathfinding
        List<Vector2Int> _outOfRangeTargets = new List<Vector2Int>();

        // Статический счетчик для присвоения уникальных номеров юнитам этого типа
        private static int _unitCounter = 0;
        // Уникальный номер текущего экземпляра юнита
        private int _unitNumber;
        // Максимальное количество целей для "умного" распределения между юнитами
        private const int MaxTargetsForSmartSelection = 3;

        // Конструктор класса - вызывается при создании каждого нового юнита
        public SecondUnitBrain()
        {
            _unitNumber = _unitCounter; // Присваиваем текущий номер из статического счетчика
            _unitCounter++; // Увеличиваем счетчик для следующего юнита
        }

        // Метод генерации снарядов при атаке
        // forTarget - цель для выстрела
        // intoList - список, куда добавляются созданные снаряды
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature; // Локальная переменная порога перегрева

            // Если оружие перегрето, не создаем снаряды
            if (GetTemperature() >= overheatTemperature)
            {
                return; // Выходим из метода без создания снарядов
            }

            // Количество снарядов зависит от текущей температуры: температура + 1
            int currentTemp = GetTemperature(); // Получаем текущую температуру как целое число
            int projectileCount = currentTemp + 1; // Вычисляем количество снарядов для выстрела

            // Создаем указанное количество снарядов
            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget); // Создаем снаряд, направленный в цель
                AddProjectileToList(projectile, intoList); // Добавляем снаряд в выходной список
            }

            IncreaseTemperature(); // Увеличиваем температуру оружия после выстрела
        }

        // Метод определения следующего шага движения юнита с использованием A*
        public override Vector2Int GetNextStep()
        {
            // Если юнит может атаковать цель в зоне досягаемости, остаемся на месте
            if (HasTargetsInRange())
                return unit.Pos;

            Vector2Int target; // Цель для движения

            // Определяем цель для движения
            if (_outOfRangeTargets.Count > 0)
            {
                target = _outOfRangeTargets[0]; // Берем первую цель из списка целей вне зоны атаки
            }
            else
            {
                // Если нет конкретных целей, идем к базе противника
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                target = runtimeModel.RoMap.Bases[enemyId]; // Позиция вражеской базы
            }

            // Если мы уже на цели, остаемся на месте
            if (unit.Pos == target)
                return unit.Pos;

            // Создаем A* путь от текущей позиции до цели
            ActivePath = new AStarUnitPath(runtimeModel, unit.Pos, target);

            // Получаем следующий шаг по вычисленному пути
            Vector2Int nextStep = ActivePath.GetNextStepFrom(unit.Pos);

            // Проверяем валидность шага (разница не должна превышать 1 клетку)
            Vector2Int delta = nextStep - unit.Pos;
            if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
            {
                // Если шаг невалидный, используем прямое движение к цели
                Debug.LogWarning($"SecondUnitBrain: invalid A* step from {unit.Pos} to {nextStep}, using fallback");
                nextStep = unit.Pos.CalcNextStepTowards(target);
            }

            // Проверяем проходимость выбранной клетки
            if (!IsCellWalkable(nextStep))
            {
                Debug.LogWarning($"SecondUnitBrain: cell {nextStep} is not walkable, finding alternative");

                // Ищем альтернативный шаг среди соседних клеток
                Vector2Int[] directions = new Vector2Int[]
                {
                    Vector2Int.up,    // Проверяем клетку сверху
                    Vector2Int.right, // Проверяем клетку справа
                    Vector2Int.down,  // Проверяем клетку снизу
                    Vector2Int.left   // Проверяем клетку слева
                };

                bool foundAlternative = false; // Флаг нахождения альтернативы
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int alternativeStep = unit.Pos + direction;
                    if (IsCellWalkable(alternativeStep)) // Если клетка проходима
                    {
                        nextStep = alternativeStep; // Используем альтернативный шаг
                        foundAlternative = true; // Устанавливаем флаг
                        break; // Выходим из цикла
                    }
                }

                if (!foundAlternative) // Если альтернатива не найдена
                {
                    Debug.LogWarning($"SecondUnitBrain: no alternative step from {unit.Pos}, staying in place");
                    return unit.Pos; // Остаемся на месте
                }
            }

            return nextStep; // Возвращаем валидный следующий шаг
        }

        // Метод выбора целей для атаки
        protected override List<Vector2Int> SelectTargets()
        {
            // Очищаем список целей для движения перед новым выбором
            _outOfRangeTargets.Clear();

            // Получаем все возможные цели (юниты противника + база)
            List<Vector2Int> allTargets = new List<Vector2Int>(GetAllTargets());
            List<Vector2Int> result = new List<Vector2Int>(); // Результирующий список целей для атаки

            // Если целей на поле нет, нацеливаемся на базу противника
            if (allTargets.Count == 0)
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyId];

                // Проверяем, находится ли база в зоне досягаемости атаки
                if (IsTargetInRange(enemyBasePos))
                {
                    result.Add(enemyBasePos); // Добавляем базу как цель для атаки
                    return result;
                }
                else
                {
                    _outOfRangeTargets.Add(enemyBasePos); // Добавляем базу в список для движения
                    return result;
                }
            }

            // Сортируем цели по расстоянию до нашей базы (приоритет - ближайшие)
            SortByDistanceToOwnBase(allTargets);

            // Распределяем цели между юнитами для избежания скучивания на одной цели
            int targetIndex;
            if (allTargets.Count <= MaxTargetsForSmartSelection) // Если целей мало (≤ 3)
            {
                targetIndex = _unitNumber % allTargets.Count; // Распределяем по номеру юнита
            }
            else // Если целей много (> 3)
            {
                targetIndex = _unitNumber % MaxTargetsForSmartSelection; // Берем остаток от деления на 3
                if (targetIndex >= allTargets.Count) // Если индекс выходит за границы
                {
                    targetIndex = allTargets.Count - 1; // Корректируем на последний валидный индекс
                }
            }

            // Получаем выбранную цель по вычисленному индексу
            Vector2Int selectedTarget = allTargets[targetIndex];

            // Проверяем, находится ли выбранная цель в радиусе атаки
            if (IsTargetInRange(selectedTarget))
            {
                result.Add(selectedTarget); // Добавляем цель для атаки
            }
            else
            {
                _outOfRangeTargets.Add(selectedTarget); // Добавляем цель для движения к ней

                // Ищем первую цель в списке, которая находится в зоне досягаемости
                foreach (Vector2Int target in allTargets)
                {
                    if (IsTargetInRange(target)) // Если цель доступна для атаки
                    {
                        result.Add(target); // Добавляем как цель для атаки
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
            if (_overheated) // Если оружие перегрето
            {
                _cooldownTime += Time.deltaTime; // Увеличиваем время остывания
                float t = _cooldownTime / (OverheatCooldown / 10); // Вычисляем прогресс остывания (от 0 до 1)
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t); // Плавно уменьшаем температуру

                if (t >= 1) // Если остывание завершено
                {
                    _cooldownTime = 0; // Сбрасываем таймер остывания
                    _overheated = false; // Снимаем флаг перегрева
                }
            }
        }

        // Вспомогательный метод получения текущей температуры как целого числа
        private int GetTemperature()
        {
            if (_overheated) // Если оружие перегрето
                return (int)OverheatTemperature; // Возвращаем максимальную температуру (3)
            else // Если не перегрето
                return (int)_temperature; // Возвращаем текущую температуру
        }

        // Вспомогательный метод увеличения температуры после выстрела
        private void IncreaseTemperature()
        {
            _temperature += 1f; // Увеличиваем температуру на 1 градус
            if (_temperature >= OverheatTemperature) // Если достигнут порог перегрева
                _overheated = true; // Устанавливаем флаг перегрева
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

            // Клетка проходима (не проверяем занятость юнитами для конечной точки)
            return true;
        }
    }
}