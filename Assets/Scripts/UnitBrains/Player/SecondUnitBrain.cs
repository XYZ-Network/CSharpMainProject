using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        List<Vector2Int> unreachableTargets = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();
            if (temp >= overheatTemperature)
            {
                return;
            }
            IncreaseTemperature();
            for (int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                Debug.Log("ВЫСТРЕЛ");
            }
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int target = unreachableTargets.Count > 0 ? unreachableTargets[0] : unit.Pos;
            //В переменную присваиваем значение, в зависимости от наличия таргетов в списке.

            return IsTargetInRange(target) ? unit.Pos : unit.Pos.CalcNextStepTowards(target);
            // Если можем дотянуться до таргета, то возвращаем позицию юнита. В противном случае возвращаем позицию следующего шага к таргету.
        }

        protected override List<Vector2Int> SelectTargets() 
        {
            List<Vector2Int> result = new();
            float minDistance = float.MaxValue;
            Vector2Int nearestTarget = Vector2Int.zero;
            Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            foreach (var target in GetAllTargets()) //Проходим циклом по вообще всем таргетам
            {
                float distanceToCurrentTarget = DistanceToOwnBase(target); // Высносим в переменную расстояние от таргета до базы

                if (minDistance > distanceToCurrentTarget) // Если переменная для рассчетов больше, чем расстояние от таргета до базы
                {
                    minDistance = distanceToCurrentTarget; // То присваиваем в эту переменную значение расстояния от таргета до базы
                    nearestTarget = target; // В переменную ближайшего таргета присваиваем координаты текущего таргета
                }
            }

            unreachableTargets.Clear(); // Убираем из списка возможные остатки с прошлых проходок

            if (minDistance < float.MaxValue) // Сравниваем изменилась ли переменная с момента её инициализции.
                // Если изминилась, значит таргеты в GetAllTargets() были
            {
                unreachableTargets.Add(nearestTarget); // Добавляем ближайший таргет в список далёких таргетов
                if (IsTargetInRange(nearestTarget)) // если можем до него дотянуться, то добавляем в резалт
                {
                    result.Add(nearestTarget);
                }
            }
            else // Если не изменилась, значит таргетов кроме базы нет.
            {
                unreachableTargets.Add(enemyBase); // Добавляем базу в список unreachableTargets
            }

            return result; // Возвращаем результат. Он может быть либо пустым, либо с таргетом(не с базой) 
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
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
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}