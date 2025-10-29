using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GluonGui.Dialog;
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
        private List<Vector2Int> priorityEnemy = new();

        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float temperature = GetTemperature();
            if (temperature >= overheatTemperature)
            {
                return;
            }
            for (int i = 0; i <= temperature; i++)
            {
                var projectile = CreateProjectile(forTarget);

                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////  
            ///////////////////////////////////////

        }
        
        

        public override Vector2Int GetNextStep()
        {


            if (priorityEnemy.Count == 0)
                return unit.Pos;
            else if (GetReachableTargets().Contains(priorityEnemy[0]))
                return unit.Pos;
            else
                return unit.Pos.CalcNextStepTowards(priorityEnemy[0]);
        }   
            

        protected override List<Vector2Int> SelectTargets()
        {
            priorityEnemy.Clear();
            List<Vector2Int> result = GetAllTargets().ToList();
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////


            if (result.Any())
            {
                List<Vector2Int> _dis = new List<Vector2Int>();


                foreach (Vector2Int enemy in result)
                {
                    _dis.Add(enemy);
                }

                SortByDistanceToOwnBase(_dis);

                int cur_Number = _unitNumber % _smartMaxTargets;
                Vector2Int current = _dis.Count >= _smartMaxTargets ? _dis[cur_Number % _dis.Count] : _dis[0];

                if (IsTargetInRange(current)) result.Add(current);
                else EnemyToAttackList.Add(current);

            }
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