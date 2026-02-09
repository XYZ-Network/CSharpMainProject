using System.Collections.Generic;
using Model.Runtime.Projectiles;
using TMPro;
using UnitBrains.Pathfinding;
using UnityEngine;
using Model;
using Utilities;
using System.Linq;
using UnityEngine.UIElements;

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
        private List<Vector2Int> UnreachableTargets = new List<Vector2Int>();
        
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
                for (int i = 0; i <= GetTemperature(); i++)
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
            if (UnreachableTargets.Count == 0 || GetReachableTargets().Contains(UnreachableTargets[0]))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(UnreachableTargets[0]);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            UnreachableTargets.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();
            if (result.Count > 1)
            {
                float min = float.MaxValue;
                Vector2Int minresult = new Vector2Int();
                foreach (Vector2Int r in result)
                {
                    if (DistanceToOwnBase(r) < min)
                    {
                        min = DistanceToOwnBase(r); //Ближайшее расстояние от цели до базы
                        minresult = r; //Координаты ближайшей до базы цели
                    }

                }
                result.Clear();
                UnreachableTargets.Add(minresult);
                if (GetReachableTargets().Contains(minresult))
                    result.Add(minresult);
            }
            else
            {
                result.Clear();
                var enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                UnreachableTargets.Add(enemyBase);
                result.Add(enemyBase);
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