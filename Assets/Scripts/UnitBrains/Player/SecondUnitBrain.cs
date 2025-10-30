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
        Vector2Int one = new Vector2Int(1, 1);
        public override string TargetUnitName => "Cobra Commando";
        private Vector2Int TargetsToAttack = new Vector2Int();
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
            float cur_temp = GetTemperature();
            if (cur_temp >= OverheatTemperature) return;
            for (int i = 0; i <= cur_temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
           return IsTargetInRange(TargetsToAttack) ? unit.Pos : unit.Pos.CalcNextStepTowards(TargetsToAttack);
        }

        private Vector2Int GetCloserTarget(IEnumerable<Vector2Int> targets)
        {
            Vector2Int min_result = targets.First();

            foreach (Vector2Int target in targets)
            {
                if (DistanceToOwnBase(min_result) > DistanceToOwnBase(target))
                {
                    min_result = target;
                }
            }
            return min_result;
        }
        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            var allTargets = GetAllTargets();

            List<Vector2Int> result = new List<Vector2Int>();

            Vector2Int min_result = GetCloserTarget(allTargets);

            if (IsTargetInRange(min_result))
            {
                TargetsToAttack = min_result;
                result.Add(min_result);
            }
            else
            {
                TargetsToAttack = min_result;
            }

            if (result.Count() == 0)
            {
                var targetBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                if (!IsTargetInRange(targetBase))
                {
                    TargetsToAttack = targetBase;
                }
                else
                { 
                    TargetsToAttack = targetBase;
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