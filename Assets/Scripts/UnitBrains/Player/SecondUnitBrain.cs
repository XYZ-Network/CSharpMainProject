using System;
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
        private static int _unitCounter = 0;
        private int _unitNumber = _unitCounter++;
        
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int SmartTargetingFactor = 3;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _priorityTargets = new();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            int currentTemperature = GetTemperature();
            if (currentTemperature >= OverheatTemperature) 
                return;
            
            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            var target = _priorityTargets.Any() 
                ? _priorityTargets.First() 
                : runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            return IsTargetInRange(target) 
                ? unit.Pos 
                : unit.Pos.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            // Enemy base already in GetAllTargets(). No need to add it explicitly.
            var result = GetAllTargets().ToList();
            SortByDistanceToOwnBase(result);
            
            // In case if number of targets is less than smart targeting factor
            var divider = Math.Min(result.Count, SmartTargetingFactor);
            var target = result[_unitNumber % divider];
            
            _priorityTargets.Clear();
            _priorityTargets.Add(target);
            
            result.Clear();
            if (IsTargetInRange(target))
                result.Add(target);
            
            return result;
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