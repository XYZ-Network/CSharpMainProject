using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
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

        List<Vector2Int> unReachableTargets = new List<Vector2Int>();

        private static int idValue = 0; // 
        private const int MaxTargets = 4; // максимум целей для умного выбора
        private int unitId;
        
        public SecondUnitBrain() // добавляем конструктор
        {
            unitId = idValue++;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            /////////////////////////////////////// 
            if (overheatTemperature <= GetTemperature()) return;

            IncreaseTemperature();

            for (int i = 0; i < GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (unReachableTargets.Count > 0)
            {
                if (IsTargetInRange(unReachableTargets[0]))
                {
                    return unit.Pos;
                }
                else
                {
                    return CalcNextStepTowards(unReachableTargets[0]);
                }
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>();
            //int checkValue = int.MaxValue;              
            Vector2Int bestTarget = new Vector2Int(0,0);
            int enemyId = 0;
            unReachableTargets.Clear();
            foreach (var target in GetAllTargets())
            {
                unReachableTargets.Add(target);
                //int distance = (int)DistanceToOwnBase(target);
                //if (distance < checkValue)
                //{
                //    checkValue = distance;
                //    bestTarget = target;
                //}
            }            

            result.Clear();

            if(unReachableTargets.Count == 0) result.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            SortByDistanceToOwnBase(unReachableTargets);
            enemyId = unitId % Mathf.Min(MaxTargets, unReachableTargets.Count);
            bestTarget = unReachableTargets[enemyId];
            if (IsTargetInRange(bestTarget)) result.Add(bestTarget);
            //if (checkValue < int.MaxValue)
            //{
            //    unReachableTargets.Add(bestTarget);
            //    if (IsTargetInRange(bestTarget)) result.Add(bestTarget);
            //}
            //else
            //{
            //    result.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            //}

            //result.Clear();
            //if(checkValue < int.MaxValue) result.Add(bestTarget);
            //while (result.Count > 1)
            //{
            //    result.RemoveAt(result.Count - 1);
            //}
            return result;
            ///////////////////////////////////////
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