using System.Collections.Generic;
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

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temp = GetTemperature();

            if (temp >= overheatTemperature) return;

            IncreaseTemperature();

            for (int i = 0; i <= temp; i++)
            { 
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);

            }
           
        }


        public override Vector2Int GetNextStep()
        {
            List<Vector2Int> targets = SelectTargets();
            if (targets.Count > 0)
            {
                Vector2Int targetPosition = targets[0];

                if (!targets.Contains(targetPosition))
                {
                    return CalcNextStepTowards(targetPosition);
                }
                else
                {
                    return base.GetNextStep();
                }
    
        }
            return base.GetNextStep();
    }

        protected List<Vector2Int> GetAllTargets()
        {
            List<Vector2Int> allTargets = new List<Vector2Int>();

            return allTargets;
        }


        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = GetAllTargets();
            if (result.Count > 0)
            {
                List<Vector2Int> outOfRangeTargets = new List<Vector2Int>();

                float minDistance = float.MaxValue;
                Vector2Int closestTarget = Vector2Int.zero;

                foreach (Vector2Int target in result)
                {
                    var distance = DistanceToOwnBase(target);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTarget = target;
                    }

                    else
                    {
                        outOfRangeTargets.Add(target);
                    }
                }

                result.Clear();
                if (minDistance < float.MaxValue) result.Add(closestTarget);
                return result;
            }
            else
            {
                int enemyPlayerID = IsPlayerUnitBrain ? runtimeModel.BotPlayerId : runtimeModel.PlayerID;
                List<Base> enemyBases = runtimeModel.RoMap.Bases[enemyPlayerID];

                if (enemyBases.Count > 0)
                {
                    result.Add(enemyBases[0].position);
                }

                return result;
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