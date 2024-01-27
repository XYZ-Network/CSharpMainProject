using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
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
        private List<Vector2Int> _unreachableTargets = new ();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
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
            if (SelectTargets().Any())
                return unit.Pos;
                
            return CalcNextStepTowards(_unreachableTargets.First());
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            var closestEnemy = Vector2Int.zero;
            var minDistanceToBase = float.MaxValue;

            if (allTargets.Any())
            {
                foreach (var enemy in allTargets)
                {
                    float distanceToBase = DistanceToOwnBase(enemy);

                    if (distanceToBase < minDistanceToBase)
                    {
                        minDistanceToBase = distanceToBase;
                        closestEnemy = enemy;
                    }
                }
            }
            else
            {
                if (IsPlayerUnitBrain)
                    closestEnemy = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
            }
            
            _unreachableTargets.Clear();
            _unreachableTargets.Add(closestEnemy);

            if (GetReachableTargets().Contains(closestEnemy))
                result.Add(closestEnemy);

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