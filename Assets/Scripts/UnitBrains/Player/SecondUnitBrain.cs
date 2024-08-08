using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnitBrains.Pathfinding;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        //public static int UnitCounter = 0;
        public const int MaxTargets = 3;
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        //private int UnitID = UnitCounter++;

        private List<Vector2Int> unreachableTargets = new();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            
            var currentTemperature = GetTemperature();
            if (currentTemperature >= overheatTemperature) return;
            
            for (int i = 0; i <= currentTemperature; i++) {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            base.GetNextStep();
            return unreachableTargets.Any()
               ? unit.Pos.CalcNextStepTowards(unreachableTargets.First())
               : unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            
            List<Vector2Int> reachableTargets = new();
            unreachableTargets.Clear();
            var allTargets = new List<Vector2Int>(GetAllTargets());

            if (!allTargets.Any()) {
                reachableTargets.Add(
                    runtimeModel.RoMap.Bases[
                        IsPlayerUnitBrain 
                            ? Model.RuntimeModel.BotPlayerId 
                            : Model.RuntimeModel.PlayerId
                        ]
                );
                return reachableTargets;
            }

            SortByDistanceToOwnBase(allTargets);
            var targets = allTargets.Take(MaxTargets);
            var _targetId = UnitID % targets.Count();
            
            unreachableTargets.Add(targets.ElementAt(_targetId));

            if (IsTargetInRange(targets.ElementAt(_targetId))) {
                reachableTargets.Add(targets.ElementAt(_targetId));
            }

            return reachableTargets;
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