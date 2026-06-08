using GluonGui.Dialog;
using Model;
using Model.Runtime.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitBrains.Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        static int Numbers = 0;
        int UnitNumber = -1;
        const int MaxTargets = 3;

        private bool _overheated;
        private List<Vector2Int> _moveTargets = new();
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= OverheatTemperature)
                return;
            
            IncreaseTemperature();
            
            int projectileCount = GetTemperature();
            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_moveTargets.Count == 0)
                return base.GetNextStep(); 

            Vector2Int target = _moveTargets[0];

            if (IsTargetInRange(target))
                return unit.Pos;

            _activePath = new SmartPath(runtimeModel, unit.Pos, target);
            return _activePath.GetNextStepFrom(unit.Pos);

        }


        protected override List<Vector2Int> SelectTargets()
        {
            if (UnitNumber == -1)
            {
                UnitNumber = Numbers;
                Numbers++;
            }

            List<Vector2Int> result = new();
            _moveTargets.Clear();

            List<Vector2Int> allTargets = GetAllTargets().ToList();

            if (allTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId
                ];
                allTargets.Add(enemyBase);
            }

            SortByDistanceToOwnBase(allTargets);

            int availableTargets = allTargets.Count < MaxTargets ? allTargets.Count : MaxTargets;

            int targetIndex = UnitNumber % availableTargets;

            Vector2Int selectedTarget = allTargets[targetIndex];

            if (IsTargetInRange(selectedTarget))
                result.Add(selectedTarget);
            else
                _moveTargets.Add(selectedTarget);

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
            if(_overheated) 
                return (int) OverheatTemperature;
            else 
                return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) 
                _overheated = true;
        }
    }
}