using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
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
        private List<Vector2Int> outOfReachTargets = new();
        
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

            for (int i = -1; i < currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int unitPos = unit.Pos;
            if (!outOfReachTargets.Any() || GetReachableTargets().Contains(outOfReachTargets[0]))
            {
                return unitPos;
            }

            return unitPos.CalcNextStepTowards(outOfReachTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new();
            IEnumerable<Vector2Int> allTargetPositions = GetAllTargets();
            if (!allTargetPositions.Any())
            {
                result.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
                return result;
            }
            List<Vector2Int> reachableTargetPositions = GetReachableTargets();
            float minRangeToBase = float.MaxValue;
            foreach (Vector2Int position in allTargetPositions)
            {
                var rangeToBase = DistanceToOwnBase(position);
                if (minRangeToBase > rangeToBase)
                {
                    minRangeToBase = rangeToBase;
                    outOfReachTargets.Clear();
                    outOfReachTargets.Add(position);
                    if (reachableTargetPositions.Contains(position))
                    {
                        result.Clear();
                        result.Add(position);
                    }
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