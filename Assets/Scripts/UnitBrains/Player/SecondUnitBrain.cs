using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;
using System.Linq;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 4f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        List<Vector2Int> targetsInRange = new List<Vector2Int>(); //цели в зоне досягаемости 
        List<Vector2Int> outOfRangeTargets = new List<Vector2Int>(); //цели вне зоне досягаемости 

        private static int Counter = 0; // счетчик 
        private int UnitNumber = Counter++; // номер юнита 
        private const int MaxTargets = 3;


        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;

            //Если нынешняя температура больше или равна темпераратуры перегрева -> Прерываем метод
            if (GetTemperature() >= overheatTemperature)
                return;

            for (int i = 0; i < GetTemperature(); i++)
            {
                // Создаем снаряд
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature(); // повышаем температуру до перегрева

            ///////////////////////////////////////
        }
        public override Vector2Int GetNextStep()
        {
            // Проверка, есть ли цели в зоне досягаемости 
            if (targetsInRange.Count > 0)
            {
                return unit.Pos; //стоим атакуем
            }
            // Если есть цели в зоне не досягаемости
            else if (outOfRangeTargets.Count > 0)
            {
                // Берем первую цель из outOfRangeTargets 
                Vector2Int targetPos = outOfRangeTargets[0];

                // Если цель не в пределах досягаемости, двигаемся к ней
                return CalcNextStepTowards(unit.Pos, targetPos);
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            outOfRangeTargets.Clear(); // очищаем список целей вне зоны
            targetsInRange.Clear(); // очищаем список целей в зоне

            List<Vector2Int> allTargets = GetAllTargets().ToList();

            if (allTargets.Any())
            {
                // Перебираем остальные цели, чтобы найти ближайшую
                foreach (Vector2Int target in allTargets)
                {
                    if (IsTargetInRange(target))
                    {
                        targetsInRange.Add(target); //добавляем цель в список досягаемости
                    }
                }
                if (targetsInRange.Count == 0)
                {
                    Vector2Int targetOutOfRange = allTargets[0];
                    outOfRangeTargets.Add(targetOutOfRange);
                    return new List<Vector2Int>();
                }

                int index = Mathf.Min(UnitNumber % MaxTargets, targetsInRange.Count - 1);
                Vector2Int mostDangerousTarget = targetsInRange[index];
                return new List<Vector2Int>() { mostDangerousTarget };  
            }

            return new List<Vector2Int>();
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }

        // Метод для расчета следующего шага
        private Vector2Int CalcNextStepTowards(Vector2Int currentPos, Vector2Int targetPos)
        {
            // Вычисляем разницу между текущей позицией и целевой
            Vector2Int direction = targetPos - currentPos;

            // Нормализуем направление (если расстояние > 0)
            if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
            if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;

            // Возвращаем следующую позицию
            return currentPos + direction;
        }
    }
}