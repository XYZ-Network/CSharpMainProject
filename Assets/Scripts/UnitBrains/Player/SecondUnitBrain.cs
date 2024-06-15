using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using Debug = UnityEngine.Debug;

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
        private List<Vector2Int> targetOutOfRange = new();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemperature = GetTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (currentTemperature >= overheatTemperature)
            {
                //Debug.Log("Overheated");
                return;
            }
            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            //return base.GetNextStep();
            if (targetOutOfRange.Count <= 0 || IsTargetInRange(targetOutOfRange[0]))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(targetOutOfRange[0]);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.6 (1st block, 6th module)
            ///////////////////////////////////////
            List<Vector2Int> result = new();
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            Vector2Int targetBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            float minDistance = float.MaxValue;
            Vector2Int closestTarget = Vector2Int.zero;
            result.Add(targetBase);
            //targetOutOfRange.Add(targetBase);

            foreach (var target in allTargets)
            {
                if (DistanceToOwnBase(target) < minDistance)
                {
                    closestTarget = target;
                    minDistance = DistanceToOwnBase(target);
                }
            }

            if (minDistance < float.MaxValue)
            {
                result.Clear();
                targetOutOfRange.Clear();

                if (IsTargetInRange(closestTarget))
                {
                    result.Add(closestTarget);
                    //if (closestTarget.Equals(targetBase)) Debug.Log("TARGET IS THE BASE");
                }
                else
                {
                    targetOutOfRange.Add(closestTarget);
                    //if (targetOutOfRange.Count() > 0) Debug.Log($"OUT OF RANGE IS NOT EMPTY");
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