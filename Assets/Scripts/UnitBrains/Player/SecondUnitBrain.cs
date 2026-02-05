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
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _unreacheableTargets = new();
        
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
            if (_unreacheableTargets.Count > 0)
            {
                return unit.Pos.CalcNextStepTowards(_unreacheableTargets.First());
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            List<Vector2Int> result = new List<Vector2Int>();
            
            if (allTargets.Any())
            {
                Vector2Int mostDangerousTarget = allTargets
                    .OrderBy(DistanceToOwnBase)
                    .First();
                if (IsTargetInRange(mostDangerousTarget))
                {
                    result.Add(mostDangerousTarget);
                }
                else
                {
                    _unreacheableTargets.Add(mostDangerousTarget);
                }
            }
            else
            {
                result.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
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