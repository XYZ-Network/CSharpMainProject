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
        private List<Vector2Int> _dangerousTargets = new();
        private bool _hasTarget = false;
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            var position = unit.Pos;
            
            if (_hasTarget || _dangerousTargets.Count == 0)
            {
                return position;
            }
            
            return position.CalcNextStepTowards(_dangerousTargets.First());
        }

        protected override List<Vector2Int> SelectTargets()
        {
            _dangerousTargets.Clear();
            
            var targets = GetAllTargets().ToList();
            Vector2Int resultEnemy;
            
            if (targets.Count == 0)
            {
                var enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                var enemyBase = runtimeModel.RoMap.Bases[enemyId];
                resultEnemy = enemyBase;
            }
            else
            {
                resultEnemy = targets.OrderBy(DistanceToOwnBase).First();
            }

            if (GetReachableTargets().Contains(resultEnemy))
            {
                _hasTarget = true;
                return new List<Vector2Int>()
                {
                    resultEnemy
                };
            }
            else
            {
                _hasTarget = false;
                _dangerousTargets.Add(resultEnemy);
                return new List<Vector2Int>();
            }
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