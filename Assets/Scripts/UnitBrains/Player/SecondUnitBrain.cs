using System;
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
        private static int UnitCount = 0;
        private const int MaxTargets = 3;
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _dangerousTargets = new();
        private int Id { get; }

        public SecondUnitBrain()
        {
            this.Id = UnitCount++;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }
            else
            {
                for(int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (_dangerousTargets.Count == 0)
            {
                return unit.Pos;
            }

            var targetNumber = GetTargetNumber();
            if (GetReachableTargets().Contains(_dangerousTargets[targetNumber]))
            {
                return unit.Pos;
            }
            else
            {
                return unit.Pos.CalcNextStepTowards(_dangerousTargets[targetNumber]);
            }
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new();
            List<Vector2Int> allTargets = new List<Vector2Int>(GetAllTargets());
            allTargets.Remove(GetEnemyBase());
            _dangerousTargets.Clear();
            if (allTargets.Any())
            {
                SortByDistanceToOwnBase(allTargets);
                _dangerousTargets.AddRange(allTargets.GetRange(0, Math.Min(allTargets.Count, MaxTargets)));
            } 
            else
            {
                _dangerousTargets.Add(GetEnemyBase());
            }
            var targetNumber = GetTargetNumber();
            if (GetReachableTargets().Contains(_dangerousTargets[targetNumber]))
            {
                result.Add(_dangerousTargets[targetNumber]);
            }
            return result;
            ///////////////////////////////////////
        }

        private int GetTargetNumber()
        {
            return Math.Min(_dangerousTargets.Count - 1, Id % MaxTargets);
        }

        protected Vector2Int GetEnemyBase()
        {
            return runtimeModel.RoMap.Bases[
            IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
        }
        public Vector2Int GetClosestToBase(IEnumerable<Vector2Int> targets)
        {
            Vector2Int closestTarget = targets.First();
            var closestDistance = DistanceToOwnBase(closestTarget);
            foreach (Vector2Int target in targets)
            {
                var distanceToBase = DistanceToOwnBase(target);
                if (distanceToBase < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = distanceToBase;
                }
            }
            return closestTarget;
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