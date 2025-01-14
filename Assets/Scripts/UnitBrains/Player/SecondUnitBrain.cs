using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
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
        private List<Vector2Int> TargetList = new List<Vector2Int>();
        private static int unitCount = 0;
        private int unitID;
        private const int maxTargets = 3; 
        
        public SecondUnitBrain()
        {
            unitID = unitCount++;
        }

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            if (GetTemperature() >= overheatTemperature)
                return;
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
                IncreaseTemperature();
            }

        }

        public override Vector2Int GetNextStep()
        {
            if (TargetList.Count > 0)
            {
                return unit.Pos.CalcNextStepTowards(TargetList[unitID % TargetList.Count]);
            }

            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTargets = GetAllTargets().ToList();

            TargetList.Clear();

            if (allTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.PlayerId : RuntimeModel.BotPlayerId];
                allTargets.Add(enemyBase);
                return TargetList;
            }

            SortByDistanceToOwnBase(allTargets);

            for (int i = 0; i < Mathf.Min(maxTargets, allTargets.Count); i++)
            {
                if (IsTargetInRange(allTargets[i]))
                {
                    result.Add(allTargets[i]);
                }
                else
                {
                    TargetList.Add(allTargets[i]);
                }
            }

            if (result.Count > 0)
            {
                TargetList.AddRange(result);
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