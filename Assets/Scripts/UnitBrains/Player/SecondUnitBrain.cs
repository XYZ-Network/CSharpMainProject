using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Model.Runtime;

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
        private static int counter = 0;
        private int UnitNumber;
        private const int MaxOfTargets = 3;
        private static int unitCounter = 0;

        public SecondUnitBrain()
        {
            UnitNumber = unitCounter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////  
            ///

            int currentTempreture = (int)GetTemperature();
            if (currentTempreture>=overheatTemperature)
            {
                return;
            }

            int projectileINT = currentTempreture;

            for (int i = 0; i < projectileINT; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

            }
            IncreaseTemperature();


            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int currentPosition = unit.Pos;
            List<Vector2Int> selectedTargets = SelectTargets();

            if (selectedTargets.Count == 0)
            {
                return currentPosition;
            }

            Vector2Int target = selectedTargets[0];


            if (Vector2Int.Distance(currentPosition, target) <= 1)// Если цель в зоне атаки не двигаемся
            {
                return currentPosition;
            }

            Vector2Int nextStep = currentPosition.CalcNextStepTowards(target);


            return nextStep;

        }





        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            ///
            List<Vector2Int> result = new List<Vector2Int>();//чистый список
            IEnumerable<Vector2Int> allTargets = GetAllTargets();//енумерабле со всеми целями
            List<Vector2Int> targetList = new List<Vector2Int>(allTargets);//енумерабле заносим в список чтоб был список со всеми целями

            
            if (!targetList.Any())// Если целей нет — атакуем базу противника
            {
                int enemyPlayerId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyPlayerId];
                result.Add(enemyBase);
                return result;
            }

            SortByDistanceToOwnBase(targetList);//сортируем по дистанции до нашей базы
            if (targetList.Count > MaxOfTargets)//ограничиваем
            {
                targetList = targetList.GetRange(0, MaxOfTargets);//диапозон
            }
            int targetIndex = UnitNumber % targetList.Count;
            Vector2Int assignedTarget = targetList[targetIndex];


            result.Add(assignedTarget);

            return result;
        }
            ///////////////////////////////////////


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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}