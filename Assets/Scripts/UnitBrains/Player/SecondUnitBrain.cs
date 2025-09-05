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
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////    
            int temperature = GetTemperature();

            if (temperature >= overheatTemperature)
            {
                return;
            }

            for(int i = 0;  i <=temperature; ++i)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }

            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_currentMovementTarget == null) return unit.Pos;

            return unit.Pos.CalcNextStepTowards(_currentMovementTarget.Value);
        }

        private Vector2Int? _currentMovementTarget;
        private List<Vector2Int> _reachableTargets = new List<Vector2Int>();

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            ///
            List<Vector2Int> result = new List<Vector2Int>();
            var allTarget = GetAllTargets().ToList();
            _reachableTargets = GetReachableTargets().ToList();

            if (allTarget.Count > 0)
            {
                Vector2Int minTarget = allTarget[0];
                float minDistance = DistanceToOwnBase(minTarget);

                for (int i = 1; i < allTarget.Count; i++)
                {
                    float currentDistance = DistanceToOwnBase(allTarget[i]);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        minTarget = allTarget[i];
                    }
                }
                if (_reachableTargets.Contains(minTarget))
                {
                    result.Add(minTarget);
                    _currentMovementTarget = null;
                }
                else _currentMovementTarget = minTarget;
            }
            else
            {
                int playerId = IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId;
                _currentMovementTarget = runtimeModel.RoMap.Bases[playerId];

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