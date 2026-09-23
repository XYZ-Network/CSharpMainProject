using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using UnityEngine;
using Utilities; // нужен для CalcNextStepTowards

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";

        // Константы для перегрева
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        // Переменные для перегрева
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        // Список целей, к которым идем
        private List<Vector2Int> _targetsToGoTo = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            //Узнаём текущую температуру
            int currentTemp = GetTemperature();

            // Если температуар слишком большая, то не стреляем
            if (currentTemp >= overheatTemperature)
            {
                return;
            }
            else
            {
                //Иначе нагреваем оружие на 1
                IncreaseTemperature();
            }

            //Узнаём новую температуру
            int newTemp = GetTemperature();

            //Количество снарядов = текущая температура
            int shotsCount = newTemp;

            //Создаём нужное количество снарядов
            for (int i = 0; i < shotsCount; i++)
            {
                BaseProjectile projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            //Если список целей пустой — стоим на месте
            if (_targetsToGoTo.Count == 0)
            {
                return unit.Pos;
            }

            //Берём первую цель из списка
            Vector2Int target = _targetsToGoTo[0];

            //Проверяем нахождение цели в зоне атаки
            bool targetInRange = IsTargetInRange(target);

            if (targetInRange == true)
            {
                //Цель рядом, не двигаемся и атакуем
                return unit.Pos;
            }
            else
            {
                //Цель далеко, то делаем один шаг в её сторону
                Vector2Int nextPosition = unit.Pos.CalcNextStepTowards(target);
                return nextPosition;
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // Создаём пустой список для результата
            List<Vector2Int> result = new List<Vector2Int>();

            //Получаем все цели
            IEnumerable<Vector2Int> allTargetsEnumerable = GetAllTargets();

            //Превращаем в список вручную
            List<Vector2Int> allTargets = new List<Vector2Int>();
            foreach (Vector2Int t in allTargetsEnumerable)
            {
                allTargets.Add(t);
            }

            //Очищаем список целей, к которым надо идти
            _targetsToGoTo.Clear();

            //Если цели есть, то ищем самую опасную (ближайшую к нашей базе)
            if (allTargets.Count > 0)
            {
                // Сортируем список по расстоянию до нашей базы
                SortByDistanceToOwnBase(allTargets);

                // После сортировки первый элемент — самый близкий к нашей базе
                Vector2Int mostDangerousTarget = allTargets[0];

                // Записываем эту цель в список для движения
                _targetsToGoTo.Add(mostDangerousTarget);

                // Проверяем, в зоне ли атаки эта цель
                bool inRange = IsTargetInRange(mostDangerousTarget);

                if (inRange == true)
                {
                    //Если в зоне атак, то добавляем в результат и будем стрелять
                    result.Add(mostDangerousTarget);
                }
            }
            else
            {
                //Если целей нет, то идём к базе противника
                int enemyId;

                // Определяем кто враг. Если мы юнит игрока, то враг бот, и наоборот
                if (IsPlayerUnitBrain == true)
                {
                    enemyId = RuntimeModel.BotPlayerId;
                }
                else
                {
                    enemyId = RuntimeModel.PlayerId;
                }

                // Получаем позицию вражеской базы
                Vector2Int enemyBasePosition = runtimeModel.RoMap.Bases[enemyId];

                // Добавляем её в список целей для движения
                _targetsToGoTo.Add(enemyBasePosition);

                // Проверяем, в зоне ли атаки
                bool inRange = IsTargetInRange(enemyBasePosition);

                if (inRange == true)
                {
                    result.Add(enemyBasePosition);
                }
            }

            // Возвращаем список целей для атаки
            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if (_overheated)
            {
                return (int)OverheatTemperature;
            }
            else
            {
                return (int)_temperature;
            }
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