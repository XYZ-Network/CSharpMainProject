using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Codice.Client.BaseCommands.CheckIn.CodeReview;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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

        public static int unitCalculate = 0;
        public int unitMarks = unitCalculate++;
        public const int MaxAimnsForSmartChoiсe = 3;
        private List<Vector2Int> NoAnyAims = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            ///// Homework 1.3
            ///////////////////////////////////////           
            var projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);

            float temp = GetTemperature();

            if (temp >= overheatTemperature) return;

            IncreaseTemperature();

            for (int i = 0; i <= temp; i++)

            {

                projectile = CreateProjectile(forTarget);

                AddProjectileToList(projectile, intoList);

            }
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            return base.GetNextStep();
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            //DZ_7//
            List<Vector2Int> result = new List<Vector2Int>();
            List<Vector2Int> allTargets = (List<Vector2Int>)GetAllTargets();
            List<Vector2Int> reachableTargets = new List<Vector2Int>();
            NoAnyAims.Clear();
            foreach (Vector2Int v2 in allTargets)
            {
                if (IsTargetInRange(v2))
                {
                    reachableTargets.Add(v2);
                }
                else
                {
                    NoAnyAims.Add(v2);
                }
            }
            if (allTargets.Count == 0 || reachableTargets.Count == 0)
            {
                Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                result.Add(enemyBase);
                return result;
            }

            SortByDistanceToOwnBase(reachableTargets);
            int targetIndex = unitMarks % MaxAimnsForSmartChoiсe;
            result.Add(reachableTargets[targetIndex]);
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