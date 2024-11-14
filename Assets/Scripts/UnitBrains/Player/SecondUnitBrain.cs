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
        private List<Vector2Int> TargetList = new List<Vector2Int>();
        private static int counter =0;
        private int UnitNumber;
        private const int MaxTargets = 3;

        public SecondUnitBrain()
        {
            UnitNumber = counter;
            counter++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {  
            float overheatTemperature = OverheatTemperature;

            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////            
            if (GetTemperature() < overheatTemperature) 
            {
                IncreaseTemperature();
                for (int i = 0; i != GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            ///////////////////////////////////////
        }

        //public override Vector2Int GetNextStep()
        //{
        //    if (TargetList.Count > 0)
        //    {
        //        return unit.Pos.CalcNextStepTowards(TargetList[0]);
        //    }
        //    return unit.Pos;
            
        //}

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = GetAllTargets().ToList();
            List<Vector2Int> AllReachableTargets = GetReachableTargets();
            int _TurgetNumber = UnitNumber < MaxTargets ? UnitNumber: UnitNumber % MaxTargets;
            Vector2Int Tmp;
            TargetList.Clear();
            if (result.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                if (IsTargetInRange(enemyBase))
                    result.Add(enemyBase);
                else
                    TargetList.Add(enemyBase);
            }
            else
            {
                SortByDistanceToOwnBase(result);
                if (AllReachableTargets.Count == 0)
                {
                    TargetList.Add((result.Count - 1) < _TurgetNumber ? result[0] : result[_TurgetNumber]);
                    result.Clear();
                }
                else
                {
                    Tmp = ((AllReachableTargets.Count - 1) < _TurgetNumber ? result[0] : result[_TurgetNumber]);
                    result.Clear();
                    result.Add(Tmp);
                }
            }
            return result;
            //////////////////////  /////////////////
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