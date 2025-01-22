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
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private List<Vector2Int> dangerousTargetsOutOfRange = new List<Vector2Int>();
        private const int limitTargetsForSmartSelect = 3;

        private static int _idValue = 0;
        public int Id { get; private set; }
        public SecondUnitBrain()
        {
            Id = _idValue++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////

            int currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
                return;
            
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
            var currentPos = unit.Pos;

            if (dangerousTargetsOutOfRange.Count() > 0)
               return currentPos.CalcNextStepTowards(dangerousTargetsOutOfRange[0]);

            return currentPos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];

            allTargets.Remove(enemyBasePos);
            if (!allTargets.Any())
                allTargets.Add(enemyBasePos);
            else
                SortByDistanceToOwnBase(allTargets);

            int indxPriorityTarget = Id % Math.Min(limitTargetsForSmartSelect, allTargets.Count());
            Vector2Int priotityTarget = allTargets[indxPriorityTarget];

            List<Vector2Int> result = new List<Vector2Int>();
            dangerousTargetsOutOfRange.Clear();

            if (GetReachableTargets().Contains(priotityTarget))
                result.Add(priotityTarget);
            else
                dangerousTargetsOutOfRange.Add(priotityTarget);

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