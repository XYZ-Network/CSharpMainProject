using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime;
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
        public List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
        private static int idCounter = -1;
        private int id = idCounter++;
        private int EnemyCount = 0;
        private int MaxCount = 3;

        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            if (GetTemperature() >= overheatTemperature) return;
            IncreaseTemperature();
            for (int i = 1; i <= (int)GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int unitPos = unit.Pos;
            if (!UnreachableTargets.Any() || GetReachableTargets().Contains(UnreachableTargets[0]))
            {
                return unitPos;
            }

            return unitPos.CalcNextStepTowards(UnreachableTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new();
            var allTargets = GetAllTargets();
            float min = float.MaxValue;
            int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
            var bestTarget = allTargets.First();
            Vector2Int mainResult = new();
            result.Clear();
            if (allTargets.Any())
            {
                foreach (var target in allTargets)
                {
                    result.Add(target);
                }
            }
            else
            {
                result.Add(runtimeModel.RoMap.Bases[enemyId]);
                return result;
            }
                SortByDistanceToOwnBase(result);
            
            foreach(var target in result)
            {
                int index;
                if (result.Count > MaxCount)
                    index = id % MaxCount;
                else
                    index = id % result.Count;
                mainResult = result[index];
            }
            result.Clear();
            UnreachableTargets.Clear();
            if (GetReachableTargets().Contains(mainResult))
                result.Add(mainResult);
            else if (!UnreachableTargets.Contains(mainResult))
                UnreachableTargets.Add(mainResult);

                return result;
            ///////////////////////////////////////
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