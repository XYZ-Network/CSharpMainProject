using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Model;
using Model.Runtime.Projectiles;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

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
        private List<Vector2Int> _currentTargets = new List<Vector2Int>();


        ///////////////////////////////////////
        // Homework 1.3 (1st block, 3rd module)
        ///////////////////////////////////////   
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {


            float overheatTemperature = OverheatTemperature;

            // Реализуй механику нагрева оружия.
            // a.Тебе необходимо проверить, а не перегрелось ли уже оружие.
            // Для этого получи текущую температуру с помощью метода GetTemperature,
            // который уже реализован в данном скрипте. И если текущая температура,
            // возвращенная этим методом больше или равна температуре перегрева - overheatTemperature,
            // то выполнение метода следует прервать, до остывания оружия,
            // которое происходит автоматически.

            float currentTemperature = GetTemperature();


            if (currentTemperature >= overheatTemperature) return;

            // Реализуй механику увеличения снарядов с каждым выстрелом.
            // a.Обрати внимание на код в методе GenerateProjectiles, который
            // генерирует снаряды и добавляет их в некий лист.Тебе необходимо
            // обернуть его в цикл так, чтобы при каждом выстреле количество
            // снарядов соответствовало текущей температуре оружия.
            // b.Какой цикл для этого больше подходит - реши самостоятельно.

            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

                // b.Каждый вызов метода GenerateProjectiles соответствует одному выстрелу.
                // С каждым выстрелом нагрев оружия должен увеличиваться.Для этого используй
                // метод IncreaseTemperature, он также уже реализован в данном скрипте, тебе
                // необходимо лишь его вызвать.                        
            }
            IncreaseTemperature();


            ///////////////////////////////////////
        }

        //public override Vector2Int GetNextStep()
        //{
        //    List<Vector2Int> result = new List<Vector2Int>();
        //    List<Vector2Int> _allTargets = new List<Vector2Int>();
        //    return base.GetNextStep();
        //}

        public override Vector2Int GetNextStep()

        {
            Vector2Int target = _currentTargets.Count > 0 ? _currentTargets[0] : unit.Pos;
            if (IsTargetInRange(target))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(target);
            }

        }


        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            float minDistance = float.MaxValue;
            Vector2Int bestTarget = Vector2Int.zero;

            foreach (Vector2Int target in allTargets)
            {
                float distance = DistanceToOwnBase(target);
                // Если цель вне зоны досягаемости, добавляем ее в коллекцию целей, к которым нужно идти

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestTarget = target;
                }
            }

            _currentTargets.Clear();

            if (minDistance < float.MaxValue)
            {
                _currentTargets.Add(bestTarget);
                if (IsTargetInRange(bestTarget)) result.Add(bestTarget);
            }
            else
            {
                // Если нет доступных целей, добавляем базу противника в качестве цели
                int enemyBaseId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                _currentTargets.Add(runtimeModel.RoMap.Bases[enemyBaseId]);
            }



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
