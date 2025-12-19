using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Model.Runtime.Projectiles;
using UnityEngine;
using System.Linq;
using GluonGui.Dialog;
using Model;
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

        Vector2Int targetsAttacked = new Vector2Int();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float TemperatureAtTheTime = GetTemperature();

            //То самое ДЗ 1.3
            if (TemperatureAtTheTime >= overheatTemperature) 
                return;
            
                            
                for (float i = 0; i <= TemperatureAtTheTime; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                } 
                IncreaseTemperature(); 
            
            
        }

        public override Vector2Int GetNextStep()
        {
            return IsTargetInRange(targetsAttacked) ? unit.Pos : unit.Pos.CalcNextStepTowards(targetsAttacked);
        }

        private Vector2Int GetCloserTarget(IEnumerable<Vector2Int> targets)
        {
            Vector2Int minRes = targets.First();
            foreach(Vector2Int target in targets)
            {
                if(DistanceToOwnBase(minRes) > DistanceToOwnBase(target))
                {
                    minRes = target;
                }
            }
            return minRes;
        }
        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            //////////////////////////////////////
            var allTargets = GetAllTargets();
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int minRes = GetCloserTarget(allTargets);

            if(IsTargetInRange(minRes))
            {
                targetsAttacked = minRes;
                result.Add(minRes); 
            }
            else
            targetsAttacked = minRes;

            if(result.Count == 0)
            {
                var targetBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                if (!IsTargetInRange(targetBase))
                targetsAttacked = targetBase;
                else
                {
                    targetsAttacked = targetBase;
                    result.Add(targetBase);
                }
            }
            

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