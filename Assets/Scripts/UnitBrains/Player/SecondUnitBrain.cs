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
        private List<Vector2Int> targetsOutOfRange = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float currentTemperature = GetTemperature();
            if (currentTemperature < OverheatTemperature)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
                IncreaseTemperature();
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (targetsOutOfRange.Count > 0)     
            {
                Vector2Int currentTarget = targetsOutOfRange[0]; 
               
                if (IsTargetInRange(currentTarget))                   
                {
                    return unit.Pos;                                 
                }
                return unit.Pos.CalcNextStepTowards(currentTarget); 
            }
            return unit.Pos;                                       
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> allTargets = GetAllTargets() as List<Vector2Int>;
            List<Vector2Int> result = new List<Vector2Int>();

            if (allTargets != null && allTargets.Count > 0)
            {
                float closestDistance = float.MaxValue;
                Vector2Int closestTarget = Vector2Int.zero;

                foreach (var target in allTargets)
                {
                    float distance = DistanceToOwnBase(target);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target;
                    }
                }

                if (IsTargetInRange(closestTarget))
                {
                    result.Add(closestTarget);
                }
                else
                {
                    targetsOutOfRange.Add(closestTarget);
                }
            }
            else
            {
                int enemyPlayerId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[enemyPlayerId];

                if (IsTargetInRange(enemyBase))
                {
                    result.Add(enemyBase);
                }
                else
                {
                    targetsOutOfRange.Add(enemyBase);
                }
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += deltaTime;
                float t = _cooldownTime / OverheatCooldown;
                _temperature = Mathf.Lerp(OverheatTemperature, 0f, t);
                if (t >= 1f)
                {
                    _cooldownTime = 0f;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            return _overheated ? (int)OverheatTemperature : (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}
