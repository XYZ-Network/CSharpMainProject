using Codice.Client.Common.FsNodeReaders;
using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public List<Vector2Int> mostDangerousTargets = new(); // Новое поле для хранения целей, к которым нужно идти, но которые вне зоны досягаемости
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////     
            int currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature)
            {
                return;
            }
            else
            {
                IncreaseTemperature();
            }
            currentTemperature = GetTemperature();
            for (int i = 0; i < currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            //Получить цель из списка целей
            if ((mostDangerousTargets.Count == 0) || isReachable(mostDangerousTargets[0])) // Если цели нет либо вне области атаки
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(mostDangerousTargets[0]); //Идти к следующей цели
            }            
            //return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////

            mostDangerousTargets.Clear();
            List<Vector2Int> result = new List<Vector2Int>(); 
            List<Vector2Int> allTargets = GetAllTargets().ToList<Vector2Int>(); 
            float minDistance = float.MaxValue;

            Vector2Int mostDangerousTarget = new Vector2Int(); 

            
            
            

            if (allTargets.Count > 0) 
            {                
                foreach (Vector2Int target in allTargets)// Проверяем все цели
                {
                    float distanceFromOwnBaseToTarget = DistanceToOwnBase(target);//Избавляемся от лишних вызовов метода, храня значение в локальной переменной
                    if (distanceFromOwnBaseToTarget < minDistance) // Проходимся по списку вообще всех целей и ищем самые близкие
                    {
                        minDistance = distanceFromOwnBaseToTarget;
                        mostDangerousTarget = target;
                    }                    
                }
            }
            else
            {                
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                result.Add(enemyBase);
                return result;
                
            }
            mostDangerousTargets.Add(mostDangerousTarget);// Записываем самую опасную цель в созданную коллекцию

            
            if (isReachable(mostDangerousTarget))
            {
                result.Add(mostDangerousTarget);//Если цель в зоне досягаемости, добавляем в result
            }
            //Цели всегда есть, в них есть база противника. Если она осталась одна, она и окажется mostDangerousTarget, отдельно её получать не нужно
            return result;

        }
            bool isReachable(Vector2Int target)// Сомневаюсь, что реализация подобного метода является частью задания ???
            {
            List<Vector2Int> reachableTargets = GetReachableTargets();// Метод из прошлого задания, получаем все цели в зоне досягаемости
            foreach(Vector2Int reachableTarget in reachableTargets)
                {
                    if (reachableTarget == target)// Если переданная цель совпадает с теми что получаем как досягаемые
                    {
                        return true; // Значит она тоже досягаемая
                    }
                }
                return false;
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