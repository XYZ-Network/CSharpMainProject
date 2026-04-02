using Codice.Client.BaseCommands;
using Codice.Client.Common.GameUI;
using Model;
using Model.Runtime.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using static Codice.CM.Common.CmCallContext;
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
        private List<Vector2Int> TargetToAttackList = new List<Vector2Int>();
        private const int _maxTargetsCount = 3;
        static private int _unitCounter = 0;
        private int _unitId = _unitCounter++;
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           
            
            float cur_temp = GetTemperature();
            if (cur_temp >= OverheatTemperature) return;
            for (int i = 0; i <= cur_temp; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }
        public override Vector2Int GetNextStep()
        {
            return TargetToAttackList.Count > 0 ? unit.Pos.CalcNextStepTowards(TargetToAttackList[0]) : unit.Pos;
        }
        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            TargetToAttackList.Clear();
            List<Vector2Int> result = new List<Vector2Int>();
            IEnumerable<Vector2Int> allTargets = GetAllTargets();
            if (allTargets.Any())
            {
                List<Vector2Int> temp = new List<Vector2Int>();
                foreach (Vector2Int target in allTargets)
                {
                    temp.Add(target);
                }
                SortByDistanceToOwnBase(temp);
                int cur_id = _unitId % _maxTargetsCount;
                Vector2Int current = temp.Count >= _maxTargetsCount ? temp[cur_id % temp.Count] : temp[0];
                if (IsTargetInRange(current)) result.Add(current);
                else TargetToAttackList.Add(current);
            }
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