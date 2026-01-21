using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using static UnityEngine.GraphicsBuffer;

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
            float currentTemp = GetTemperature();

            if (currentTemp >= OverheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            int projectileCount = Mathf.Min((int)currentTemp + 1, 3);

            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }


        protected override List<Vector2Int> SelectTargets
        {
            get
            {

                var result = GetReachableTargets();

                if (result.Count > 1)
                {
                    //находим ближайшую к своей базе
                    Vector2Int closestToBase = result[0];
                    float minDistanceToBase = DistanceToOwnBase(result[0]);

                    for (int i = 1; i < result.Count; i++)
                    {
                        float distanceToBase = DistanceToOwnBase(result[i]);
                        if (distanceToBase < minDistanceToBase)
                        {
                            minDistanceToBase = distanceToBase;
                            closestToBase = result[i];
                        }
                    }

                    //оставляем только ближайшую
                    result = new List<Vector2Int> { closestToBase };
                }
                else if (result.Count == 0)
                {
                    //если нет достижимых целей добывляем базу противника
                    var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                    result.Add(enemyBase);
                }
                return result;
            }
        }

        private List<Vector2Int> _outOfRangeTargets = new List<Vector2Int>();
        protected List<Vector2Int> GetReachableTargets()
        {
            List<Vector2Int> allTargets = GetReachableTargets(); 
            List<Vector2Int> result = new List<Vector2Int>();

            if (allTargets.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];
                result.Add(enemyBase);
                return result;
            }

            Vector2Int mostDangerousTarget = allTargets[0];
            float minDistanceToBase = DistanceToOwnBase(allTargets[0]);

            for (int i = 1; i < allTargets.Count; i++)
            {
                float distanceToBase = DistanceToOwnBase(allTargets[i]);
                if (distanceToBase < minDistanceToBase)
                {
                    minDistanceToBase = distanceToBase;
                    mostDangerousTarget = allTargets[i];
                }
            }

            if (IsTargetReachable(mostDangerousTarget))
            {
                result.Add(mostDangerousTarget);
            }
            else
            {
                _outOfRangeTargets.Clear();
                _outOfRangeTargets.Add(mostDangerousTarget); 
            }

            return result.Count > 0 ? result : _outOfRangeTargets;
        }

        private bool IsTargetReachable(Vector2Int target)
        {
            return GetReachableTargets().Contains(target);
        }
        public override Vector2Int GetNextStep()
        {
            List<Vector2Int> targets = SelectTargets;

            if (targets.Count == 0)
            {
                return base.GetNextStep();
            }

            Vector2Int target = targets[0]; 
            bool isInAttackRange = true;
            Vector2Int position = Vector2Int.zero;
            Vector2Int nextposition = Vector2Int.right;
            position = position.CalcNextStepTowards(target);


            if (isInAttackRange)
            {
                return base.GetNextStep(); ;
            }
            else
            {
                return position. CalcNextStepTowards(target);
            }
        }
       
        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
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
            if (_overheated) return (int)OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}