using System.Collections.Generic;
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
        private List<Vector2Int> _currentTargets = new List<Vector2Int>();

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
            List<Vector2Int> target = Vector2Int.zero;
            if (_currentTargets.Count > 0) target = _currentTargets[0];
            else target = unit.Pos;
            if (IsTargetInRange(target)) return unit.Pos;
            else return target;
        
    }

        protected List<Vector2Int> GetAllTargets()
        {
            List<Vector2Int> allTargets = new List<Vector2Int>();

            return allTargets;
        }


        protected override List<Vector2Int> SelectTargets()
        {

            List<Vector2Int> result = new List<Vector2Int>();
            float minDistance = float.MaxValue;
            Vector2Int closestTarget = Vector2Int.zero;

                foreach (Vector2Int target in GetAllTargets())
                {
                    var distance = DistanceToOwnBase(target);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = target;
                }
                }

                _currentTargets.Clear();
            if (minDistance < float.MaxValue)
            {
                _currentTargets.Add(closestTarget);
                if (IsTargetInRange(closestTarget)) result.Add(closestTarget);

            }
            else
            {
                _currentTargets.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerID : RuntimeModel.PlayerID]);
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
