using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Model;
using Model.Runtime.Projectiles;
using Unity.VisualScripting;
using UnityEngine;

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
        List<Vector2Int> outOfReachTargets = new List<Vector2Int>();
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature) 
            {
                return;
            }
            IncreaseTemperature();          
           
            
            for(int i = 0; i <= temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            
        }

        public override Vector2Int GetNextStep()
        {
            if (outOfReachTargets.Any())
            {
                if (IsTargetInRange(outOfReachTargets[0]))
                {
                    return unit.Pos;
                }
                else
                {
                    var target = CalcNextStepTowards(outOfReachTargets[outOfReachTargets.Count - 1]);
                    return target;
                }
                
            }

            return unit.Pos;
        }
        
        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = new List<Vector2Int>();
            float Min = float.MaxValue;
            Vector2Int bestTarget = Vector2Int.zero;

            foreach (Vector2Int i in GetAllTargets())
            {
                float distance = DistanceToOwnBase(i);
                                
                if (distance < Min)
                {
                    Min = distance;

                    bestTarget = i;
                }

            }
            //result.Clear();
            //if (Min < float.MaxValue) result.Add(bestTarget);
            outOfReachTargets.Clear();
            if(Min < float.MaxValue)
            {
                outOfReachTargets.Add(bestTarget);
                if(IsTargetInRange(bestTarget))
                {
                    result.Add(bestTarget);
                }
            }
            else
            {
                if(IsPlayerUnitBrain) outOfReachTargets.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            }
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