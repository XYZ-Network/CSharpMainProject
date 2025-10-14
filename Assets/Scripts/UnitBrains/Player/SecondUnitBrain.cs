using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

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
        public List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
        //////////////////////////
        private static int _unitCounter = 0; // Статичный счетчик
        private int _unitNumber = 0; // Текущий номер юнита
        private const int _unitMaxNumber = 3; // Переменная, определяющая максимум количества целей
        private List<Vector2Int> _targets = new(); // Список целей для атаки
        //////////////////////////

        public SecondUnitBrain() // Конструктор для нумерации юнита
        {
            _unitNumber = _unitCounter++;
        }
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            //////////////////////////
            if (GetTemperature() >= overheatTemperature)
                return;
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            IncreaseTemperature();
            ////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_targets.Count > 0) // Направляю юнита к противнику вне радиуса
            {
                return unit.Pos.CalcNextStepTowards(_targets[0]); // Направляется к первой цели в списке (если противников нет - направляется к базе)
            }

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////

            List<Vector2Int> allTargets = GetAllTargets().ToList(); // Список всех целей
            List<Vector2Int> result = new List<Vector2Int>(); // Список, определяющий цель

            _targets.Clear(); // Очищаем список целей

            if (allTargets.Count == 0) // Если в списке нет юнитов - атакуем базу противника
            {
                var target = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                result.Add(target);
                _targets.Add(target);
                return result;
            }
            
            SortByDistanceToOwnBase(allTargets); // Сортировка юнитов по дальности от нашей базы

            int myTargetIndex = _unitNumber % _unitMaxNumber; // Определяю номер моей цели
            if (myTargetIndex < allTargets.Count) // Если номер моей цели меньше общего числа целей
            {
                Vector2Int myPersonalTarget = allTargets[myTargetIndex]; // Объявляю переменную юнита из списка всех целей

                if (IsTargetInRange(myPersonalTarget)) // Атакуем цель, если она в радиусе
                {
                    result.Add(myPersonalTarget);
                }
                else
                {
                    _targets.Add(myPersonalTarget); // Добавляю в список для движения к цели вне радиуса
                }
            }
            return result;
        }
        /////////////////////////////////////////


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